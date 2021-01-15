using ApiCielo.Models;
using Cielo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiCielo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CieloController : ControllerBase
    {
        private readonly CieloApi _api;
        private readonly Merchant _merchant;

        public CieloController(CieloApi cieloApi, Merchant merchant)
        {
            _api = cieloApi;
            _merchant = merchant;
        }

        [HttpPost("fazer_transacao")]
        public IActionResult TransctionRequest([FromBody] TransactionData transactionData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(e => e.Errors));
            }

            //Mapeia strings em enums
            Enum.TryParse(transactionData.Payment.CreditCard.Brand, out CardBrand brand);
            Enum.TryParse(transactionData.Payment.Currency, out Currency currency);

            var customerCielo = new Customer(name: transactionData.Customer.Name)
            {
                Address = new Address()
                {
                    ZipCode = transactionData.Customer.Address.ZipCode,
                    City = transactionData.Customer.Address.City,
                    State = transactionData.Customer.Address.State,
                    Complement = transactionData.Customer.Address.Complement,
                    District = transactionData.Customer.Address.District,
                    Street = transactionData.Customer.Address.Street,
                    Number = transactionData.Customer.Address.Number,
                    Country = transactionData.Customer.Address.Country
                }
            };

            DateTime birthday = DateTime.Parse(transactionData.Customer.Birthdate);

            customerCielo.SetIdentityType(IdentityType.CPF);
            customerCielo.Identity = transactionData.Customer.Identity;
            customerCielo.SetBirthdate(birthday);
            customerCielo.Email = transactionData.Customer.Email;

            DateTime expirationDate = DateTime.Parse(transactionData.Payment.CreditCard.ExpirationDate);
            decimal test = Convert.ToDecimal(transactionData.Payment.Amount);
          
            var creditCard = new Card(
                cardNumber: transactionData.Payment.CreditCard.CardNumber,
                holder: transactionData.Payment.CreditCard.Holder,
                expirationDate: expirationDate,
                securityCode: transactionData.Payment.CreditCard.SecurityCode,
                brand: brand
                );

            var paymentCielo = new Payment(
                
                amount: test,
                currency: currency,
                installments: transactionData.Payment.Installments.Value,
                capture: transactionData.Payment.Capture.Value,
                softDescriptor: transactionData.Payment.SoftDescriptor,
                card: transactionData.Payment.CreditCard,
                returnUrl: transactionData.Payment.ReturnUrl
                );


            var merchantOrderId = transactionData.MerchantOrderId;

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customerCielo,
                payment: paymentCielo
                );
            
            try
            {
                var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

                if(returnTransaction.Payment.GetStatus() == Status.Denied)
                {
                    return BadRequest("Erro na transação");
                }
            }
            catch (CieloException ex)
            {
                return BadRequest(ex.GetCieloErrors());
            }

            return Ok();

        }
    }
}
