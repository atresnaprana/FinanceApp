using BaseLineProject.Data;
using FinanceApp.Models;
using FinanceApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using static FinanceApp.Models.Enums.TaxEnums;

namespace FinanceApp.Services.Tax
{
    public class TaxCalculationService : ITaxCalculationService
    {
        private const decimal UMKM_LIMIT = 4_800_000_000m;
        private const decimal UMKM_TAX_RATE = 0.005m;
        private const decimal PPH_BADAN_RATE = 0.22m;

        public TaxSummaryResult CalculateAnnualTax(
            string companyId,
            int year,
            IEnumerable<dbJpn> jpn,
            IEnumerable<dbJpb> jpb,
            IEnumerable<dbJm> jm, string taxFlagPercentage, DateTime registrationDate, string customertype)
        {
            // 1️⃣ OMZET = Penjualan (JPN)
            var omzetBulanan = jpn
                .Where(x => x.TransDate.Year == year && x.company_id == companyId)
                .GroupBy(x => x.TransDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Omzet = g.Sum(x => (decimal)x.Value)
                })
                .ToList();

            var totalOmzet = omzetBulanan.Sum(x => x.Omzet);
            var eligibleUMKM = totalOmzet <= UMKM_LIMIT && taxFlagPercentage == "Y" && CanUseUmkmFinal(customertype, registrationDate, year);

            var result = new TaxSummaryResult
            {
                TotalOmzet = totalOmzet,
                IsUMKMEligible = eligibleUMKM,
            };

            // 2️⃣ JIKA UMKM FINAL
            if (eligibleUMKM)
            {
                foreach (var b in omzetBulanan)
                {
                    result.Monthly.Add(new TaxMonthlyBreakdown
                    {
                        Month = b.Month,
                        Omzet = b.Omzet,
                        Tax = b.Omzet * UMKM_TAX_RATE
                    });
                }

                result.FinalTaxAmount = result.Monthly.Sum(x => x.Tax);
                return result;
            }

            // 3️⃣ NON FINAL (LABA RUGI)
            var totalRevenue = totalOmzet;

            var totalExpense =
                jpb.Where(x => x.TransDate.Year == year && x.company_id == companyId)
                   .Sum(x => (decimal)x.Value)
              + jm.Where(x => x.TransDate.Year == year && x.company_id == companyId)
                   .Sum(x => (decimal)(x.Debit - x.Credit));

            var profit = totalRevenue - totalExpense;

            result.AccountingProfit = profit;
            result.NonFinalTaxAmount = profit > 0 ? profit * PPH_BADAN_RATE : 0;

            return result;
        }
        private bool CanUseUmkmFinal(
        string customerType,
        DateTime regDate,
        int taxYear)
        {
            int yearsUsed = taxYear - regDate.Year + 1;

            switch (customerType.ToUpper())
            {
                case "PERORANGAN": return yearsUsed <= 7;
                case "CV":
                case "FIRMA": return yearsUsed <= 4;
                case "PT": return yearsUsed <= 3;
                default: return false;
            }
        }

    }
}