using BaseLineProject.Data;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace FinanceApp.Services
{
    public class TaxEligibilityService
    {
        private const decimal UMKM_THRESHOLD = 4_800_000_000m;
        private readonly FormDBContext _context;

        public TaxEligibilityService(FormDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hitung dan simpan eligibility pajak tahunan (dipanggil saat closing Desember)
        /// </summary>
        public void CalculateAnnualTaxEligibility(string company_id, int taxYear)
        {

            var salesJournals = _context.JpnTbl
                .Where(j =>
                    j.company_id == company_id &&
                    j.TransDate.Year == taxYear)
                .OrderBy(j => j.TransDate)
                .ToList();

            decimal annualGross = salesJournals.Sum(j => (decimal)j.Value);

            bool eligible = annualGross <= UMKM_THRESHOLD;
            DateTime? crossingDate = null;

            if (!eligible)
            {
                decimal runningTotal = 0m;

                foreach (var journal in salesJournals)
                {
                    runningTotal += journal.Value;

                    if (runningTotal > UMKM_THRESHOLD)
                    {
                        crossingDate = journal.TransDate.Date;
                        break;
                    }
                }
            }
            var datas = _context.CustomerTbl.Where(y => y.COMPANY_ID == company_id).FirstOrDefault();

            var existing = _context.TaxEligibilities
                .FirstOrDefault(t =>
                    t.CustomerId == datas.id &&
                    t.TaxYear == taxYear);

            if (existing == null)
            {
                _context.TaxEligibilities.Add(new TaxEligibility
                {
                    CustomerId = datas.id,
                    TaxYear = taxYear,
                    AnnualGross = annualGross,
                    EligiblePp23 = eligible ? "Y" : "N",
                    CrossingDate = crossingDate,
                    DetectionDate = DateTime.UtcNow
                });
            }
            else
            {
                existing.AnnualGross = annualGross;
                existing.EligiblePp23 = eligible ? "Y" : "N";
                existing.CrossingDate = crossingDate;
                existing.DetectionDate = DateTime.UtcNow;
            }

            _context.SaveChanges();
        }
    }
}
