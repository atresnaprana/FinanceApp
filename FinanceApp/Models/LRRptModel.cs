
using Microsoft.Identity.Client;

namespace FinanceApp.Models
{
    public class LRRptModel
    {
        public string akun { get; set; }
        public string description { get; set; }
        public string debit { get; set; }
        public string credit { get; set; }
        public string total { get; set; }
        public decimal totalint { get; set; }


    }
}
