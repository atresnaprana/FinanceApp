using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace FinanceApp.Models
{
    public class dbClosedValue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string company_id { get; set; }
        public int year { get; set; }
        public int Akun_Debit { get; set; }
        public int Akun_Credit { get; set; }

        public long debit { get; set; }
        public long credit {  get; set; }    
        public string entry_user { get; set; }
        public string update_user { get; set; }

        public DateTime update_date { get; set; }
        public DateTime entry_date { get; set; }
        public string src { get; set; }

    }
}
