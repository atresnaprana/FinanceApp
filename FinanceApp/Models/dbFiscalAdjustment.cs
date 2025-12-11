using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class dbFiscalAdjustment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public string company_id { get; set; }
        public int account_no { get; set; }

        // DEDUCTIBLE / NONDEDUCTIBLE
        public string override_fiscal_type { get; set; }

        public string reason { get; set; }

        public string entry_user { get; set; }
        public string update_user { get; set; }
        public DateTime entry_date { get; set; }
        public DateTime update_date { get; set; }

        public string flag_aktif { get; set; }
    }
}
