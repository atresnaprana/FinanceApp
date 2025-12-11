using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class dbTaxConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string taxtype { get; set; }
        public string taxmode { get; set; }
        public long taxlimit { get; set; } = 0;
        public long taxlimitmin { get; set; } = 0;
        public long taxlimitmax { get; set; } = 0;
        public decimal taxpercentage { get; set; } = 0;
        public string entry_user { get; set; }
        public string update_user { get; set; }

        public string flag_aktif { get; set; }
        public DateTime update_date { get; set; }
        public DateTime entry_date { get; set; }

    }
}
