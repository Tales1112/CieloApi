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
        private string _nome;
        private string _nomeCartao;
        private string _descricao;
        private CieloApi _api;
        private DateTime _validDate;
        private DateTime _invalidDate;
        [HttpPost("fazer_transacao")]
        public async Task<IActionResult> TransctionRequest([FromBody] TransactionData transactionData)
        {
            ISerializerJSON json = new SerializerJSON();
            Guid key = new Guid("ac1c2e84-c004-4c27-a4d6-cf67bdfbb2d3");

            Merchant merchant = new Merchant(key, "ZZIMLFOIASAUWNYTRUWEZBNPDJGOHVAODXZPWSED");
            _api = new CieloApi(CieloEnvironment.SANDBOX, Merchant.SANDBOX, json);
            _validDate = DateTime.Now.AddYears(2);
            _invalidDate = DateTime.Now.AddYears(-2);

            _nome = "Tales Silva";
            _nomeCartao = "Tales Carvalho Silva";
            _descricao = "Teste Cielo";

            CardBrand brand = CardBrand.Visa;


            var customerCielo = new Customer(name: _nomeCartao)
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
            customerCielo.Identity = transactionData.Customer.Identity; //numero gerado aleatoriamente
            customerCielo.SetBirthdate(birthday);
            customerCielo.Email = transactionData.Customer.Email;

            DateTime expirationDate = DateTime.Parse(transactionData.Payment.CreditCard.ExpirationDate);
            decimal test = Convert.ToDecimal(transactionData.Payment.Amount);
          
            var creditCard = new Card(
                cardNumber: transactionData.Payment.CreditCard.CardNumber,
                holder: transactionData.Payment.CreditCard.Holder,
                expirationDate: expirationDate,
                securityCode: transactionData.Payment.CreditCard.SecurityCode,
                brand: brand);

            var paymentCielo = new Payment(
                
                amount: test,
                currency: Currency.BRL,
                installments: transactionData.Payment.Installments.Value,
                capture: transactionData.Payment.Capture.Value,
                softDescriptor: transactionData.Payment.SoftDescriptor,
                card: transactionData.Payment.CreditCard,
                returnUrl: transactionData.Payment.ReturnUrl);

            /* store order number */

            var merchantOrderId = transactionData.MerchantOrderId;

            var transaction = new Transaction(
                merchantOrderId: merchantOrderId.ToString(),
                customer: customerCielo,
                payment: paymentCielo);

            var returnTransaction = _api.CreateTransaction(Guid.NewGuid(), transaction);

            //Consultando
            var result = _api.GetTransaction(returnTransaction.Payment.PaymentId.Value);

            return Ok();

        }
    }
}
