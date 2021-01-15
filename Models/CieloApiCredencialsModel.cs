using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiCielo.Models
{
    public class CieloApiCredencialsModel
    {
        public Guid CieloId { get; set; }
        public string CieloKey { get; set; }
    }
}
