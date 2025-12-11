using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    public class FiscalAdjustmentViewModel
    {
        // From dbFiscalAdjustment
        public int id { get; set; } // Needed for Edit/Delete

        [Display(Name = "Override Fiscal Type")]
        public string OverrideFiscalType { get; set; }
        public string Reason { get; set; }

        // From dbAccount
        [Display(Name = "Account No")]
        public int AccountNo { get; set; }

        [Display(Name = "Account Name")]
        public string AccountName { get; set; }

        [Display(Name = "Default Fiscal Type")]
        public string DefaultFiscalType { get; set; }

        // For passing data back
        public string CompanyId { get; set; }
    }
}