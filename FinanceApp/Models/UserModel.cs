using BaseLineProject.Models;
using System.Collections.Generic;
namespace FinanceApp.Models
{
    public class UserModel
    {
        public string pkg { get; set; }
        public List<dbCustomer> customerdt { get;set; }
    }
}
