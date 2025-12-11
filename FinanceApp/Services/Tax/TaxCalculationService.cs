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
        private readonly FormDBContext _context;
        public TaxCalculationService(FormDBContext context)
        {
            _context = context;
        }

        
        public TaxSummaryResult CalculateAnnualTax(
    string companyId,
    int year,
    IEnumerable<dbJpn> jpn,
    IEnumerable<dbJpb> jpb,
    IEnumerable<dbJm> jm,
    string taxFlagPercentage,
    DateTime registrationDate,
    string customertype)
        {
            // ===== 1. HITUNG OMZET =====
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

            bool isEligibleUMKM = totalOmzet <= UMKM_LIMIT
                                  && taxFlagPercentage == "Y"
                                  && CanUseUmkmFinal(customertype, registrationDate, year);

            var result = new TaxSummaryResult
            {
                TotalOmzet = totalOmzet,
                IsUMKMEligible = isEligibleUMKM
            };

            // ===== 2. JIKA FINAL (UMKM 0.5%) =====
            if (isEligibleUMKM)
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

            // ===== 3. NON-FINAL, HITUNG LABA =====
            var totalRevenue = totalOmzet;

            var totalExpense =
                jpb.Where(x => x.TransDate.Year == year && x.company_id == companyId)
                   .Sum(x => (decimal)x.Value)
                + jm.Where(x => x.TransDate.Year == year && x.company_id == companyId)
                   .Sum(x => (decimal)(x.Debit - x.Credit));

            var profit = totalRevenue - totalExpense;
            result.AccountingProfit = profit;

            // ===== 4. APLIKASIKAN LAYER PAJAK DARI DB =====
            var taxLayer = _context.TaxConfigTbl
                .Where(x => x.taxtype == "non-final" && x.flag_aktif == "1")
                .OrderBy(x => x.taxlimitmin)
                .ToList();

            // fallback default: 22% bila tak ketemu layer
            decimal applicableRate = 0.22m;

            foreach (var layer in taxLayer)
            {
                if (profit >= layer.taxlimitmin && profit <= layer.taxlimitmax)
                {
                    applicableRate = layer.taxpercentage / 100m;
                    break;
                }
            }

            // ===== 5. FINAL RESULT =====
            result.NonFinalTaxAmount = profit > 0 ? profit * applicableRate : 0;

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