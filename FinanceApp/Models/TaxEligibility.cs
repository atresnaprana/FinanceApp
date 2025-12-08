using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    [Table("tax_eligibility")]

    public class TaxEligibility
    {
        [Key]
        [Column(Order = 0)]
        public int CustomerId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int TaxYear { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AnnualGross { get; set; }

        [Column(TypeName = "char(1)")]
        public string EligiblePp23 { get; set; } // 'Y' / 'N'

        public DateTime? CrossingDate { get; set; }

        public DateTime DetectionDate { get; set; }
    }
}
