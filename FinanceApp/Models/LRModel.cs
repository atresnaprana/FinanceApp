using OfficeOpenXml.ConditionalFormatting;
using System.Collections.Generic;

namespace FinanceApp.Models
{
    public class LRModel
    {
        public int year { get; set; }
        public int month { get; set; }
        public bool isYearly { get; set; }
        public List<LRRptModel> ReportModel { get; set; }
        public List<TBModel> TB_D { get; set; }
        public List<TBModel> TB_K { get; set; }

        public List<dbJm> jmdata { get; set; }
        public List<dbJpb> jpbdata { get; set; }
        public List<dbJpn> jpndata { get; set; }
        public List<dbClosing> closingdata { get; set; }
        public List<dbAccount> akundata { get; set; }
    }
}
