using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace FinanceApp.Models
{
    public class dbBank
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public DateTime TransDate { get; set; }
        public string Trans_no { get; set; }
        public string Description { get; set; }

        public int Akun_Debit { get; set; }
        public int Akun_Credit { get; set; }

        public int Debit { get; set; }
        public int Credit { get; set; }
        public int Saldo { get; set; }

        public string entry_user { get; set; }
        public string update_user { get; set; }

        public string flag_aktif { get; set; }

        public string TransDateStr { get; set; }
        public string MonthStr { get; set; }

        public string YearStr { get; set; }

        public DateTime update_date { get; set; }
        public DateTime entry_date { get; set; }
        [NotMapped]
        public string errormessage { get; set; }
        [NotMapped]
        public List<dbAccount> dddbacc { get; set; }
        [NotMapped]
        public string akuntype { get; set; }

        [NotMapped]
        public string shorttransno { get; set; }
        [NotMapped]
        public int lasttransno { get; set; }
        [NotMapped]
        public string DebitStr { get; set; }
        [NotMapped]
        public string CreditStr { get; set; }
        [NotMapped]
        public String SaldoStr { get; set; }
    }
}
