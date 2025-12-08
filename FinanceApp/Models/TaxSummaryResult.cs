using System.Collections.Generic;

namespace FinanceApp.Models
{
    public class TaxSummaryResult
    {
        public decimal TotalOmzet { get; set; }
        public bool IsUMKMEligible { get; set; }

        public decimal FinalTaxRate { get; set; }
        public decimal FinalTaxAmount { get; set; }

        public decimal AccountingProfit { get; set; }
        public decimal NonFinalTaxAmount { get; set; }

        public List<TaxMonthlyBreakdown> Monthly { get; set; } = new List<TaxMonthlyBreakdown>();
    }
    public class TaxMonthlyBreakdown
    {
        public int Month { get; set; }
        public decimal Omzet { get; set; }
        public decimal Tax { get; set; }
    }
}
