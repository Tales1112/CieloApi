using Cielo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiCielo.Models
{
    public class TransactionData
    {
        public string MerchantOrderId { get; set; }
        public Customer Customer { get; set; }
        public Payment Payment { get; set; }
      
    }
}
