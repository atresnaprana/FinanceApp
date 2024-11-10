using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic;
namespace FinanceApp.Models
{
    public class dbAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public int account_no { get; set; }
        public string hierarchy { get; set; }

        public string account_name { get; set; }

        public string akundk { get; set; }

        public string akunnrlr { get; set; }

        public string entry_user { get; set; }
        public string update_user { get; set; }

        public string flag_aktif{ get; set; }

        public DateTime update_date { get; set; }
        public DateTime entry_date { get; set; }

         


    }
}
