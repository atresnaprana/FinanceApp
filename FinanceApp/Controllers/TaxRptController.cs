using BaseLineProject.Data;
using BaseLineProject.Models;
using FinanceApp.Models;
using FinanceApp.Services.Tax;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using MimeDetective.Storage.Xml.v2;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Database;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace FinanceApp.Controllers
{
    public class TaxRptController : Controller
    {
        private readonly FormDBContext db;
        public const string SessionKeyName = "TransDateStr";
        private readonly ITaxCalculationService _taxService;
        private const decimal UMKM_LIMIT = 4_800_000_000m;

        private IHostingEnvironment Environment;
        private readonly ILogger<LRController> _logger;

        public TaxRptController(FormDBContext db, ILogger<TaxRptController> logger, IHostingEnvironment _environment, ITaxCalculationService taxService)
        {
            logger = logger;
            Environment = _environment;
            this.db = db;
            _taxService = taxService;


        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult GeneratePdf(string message)
        {
            ViewBag.Message = message;
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GeneratePdf(LRModel obj)
        {
            var year = obj.year;
            var maxMonth = obj.isYearly ? 12 : obj.month;

            // ✅ VALIDASI CLOSING
            var closingCount = db.ClosingTbl
                .Count(x => x.year == year && x.periode <= maxMonth && x.isclosed == "Y");

            if (closingCount != maxMonth)
                return RedirectToAction("Index", new { msg = "Closing belum lengkap" });

            // ✅ AMBIL COMPANY
            var customer = db.CustomerTbl.First(x => x.Email == User.Identity.Name);
            var companyId = customer.COMPANY_ID;

            // ✅ LOAD DATA
            var jpn = db.JpnTbl
                .Where(x => x.company_id == companyId && x.TransDate.Year == year)
                .ToList();

            var jpb = db.JpbTbl
                .Where(x => x.company_id == companyId && x.TransDate.Year == year)
                .ToList();

            var jm = db.JmTbl
                .Where(x => x.company_id == companyId && x.TransDate.Year == year)
                .ToList();
            var datacust = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            // ✅ HITUNG PAJAK
            var taxResult = _taxService.CalculateAnnualTax(
                companyId, year, jpn, jpb, jm, datacust.taxflagpercentage, datacust.REG_DATE, datacust.customertype);

            // ✅ GENERATE PDF
            var eligibleUMKM =  taxResult.TotalOmzet <= UMKM_LIMIT && datacust.taxflagpercentage == "Y" && CanUseUmkmFinal(datacust.customertype, datacust.REG_DATE, year);

            var pdfBytes = GeneratePdfFromTaxResult(taxResult, year, eligibleUMKM);
            var fileName = $"TaxRpt-{DateTime.Now:dd-MM-yyyy}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
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
        private static string GetMonthName(int month)
        {
            return new DateTime(2000, month, 1)
                .ToString("MMMM", new CultureInfo("id-ID"));
        }

        // -----------------------------------------
        // PDF PART (SINGKAT)
        // -----------------------------------------
        private byte[] GeneratePdfFromTaxResult(TaxSummaryResult tax, int year, bool iseligible)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var culture = new CultureInfo("id-ID");
            using var stream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // ---------- HEADER ----------
                    page.Header().Column(col =>
                    {
                        col.Item().Text("Report Preview Laporan Pajak")
                            .Bold().FontSize(12).AlignCenter();

                        col.Item().Text($"Tahun: {year}")
                            .FontSize(9).AlignCenter();

                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(8)
                            .FontColor(Colors.Grey.Medium)
                            .AlignCenter();

                        col.Spacing(10);
                    });

                    // ---------- CONTENT ----------
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);    // Bulan
                            columns.RelativeColumn(1.5f); // Omzet
                            columns.RelativeColumn(1.5f); // Pajak
                        });

                        void AddRow(string c1, string c2, string c3, bool isHeader = false)
                        {
                            var fontSize = isHeader ? 8 : 7;
                            var bg = isHeader ? Colors.Grey.Lighten3 : Colors.White;

                            table.Cell().Border(1).Background(bg).Padding(4)
                                .Text(c1).FontSize(fontSize);

                            table.Cell().Border(1).Background(bg).Padding(4)
                                .Text(c2).FontSize(fontSize).AlignRight();

                            table.Cell().Border(1).Background(bg).Padding(4)
                                .Text(c3).FontSize(fontSize).AlignRight();
                        }
                        if (iseligible) {
                            // Header table
                            AddRow("Bulan", "Peredaran Bruto", "Potongan Pajak", true);

                            // Data
                            foreach (var m in tax.Monthly)
                            {
                                decimal omzet = m.Omzet;

                                // ✅ FIX: kalau Tax = 0 → hitung 0.5%
                                decimal pajak = m.Tax > 0
                                    ? m.Tax
                                    : Math.Round(omzet * 0.005m, 2);

                                AddRow(
                                    GetMonthName(m.Month),
                                    omzet.ToString("N2", culture),
                                    pajak.ToString("N2", culture)
                                );
                            }

                            // ---------- TOTAL ----------
                            decimal totalOmzet = tax.Monthly.Sum(x => x.Omzet);
                            decimal totalTax = tax.Monthly.Sum(x =>
                                x.Tax > 0 ? x.Tax : x.Omzet * 0.005m);

                            AddRow(
                                "TOTAL",
                                totalOmzet.ToString("N2", culture),
                                totalTax.ToString("N2", culture),
                                true
                            );
                        }
                        else
                        {
                            decimal totalOmzet = tax.TotalOmzet;
                            decimal totalTax = tax.NonFinalTaxAmount;
                            decimal profit = tax.AccountingProfit;

                            AddRow("Peredaran Bruto", "Profit", "Potongan Pajak", true);

                            AddRow(
                                totalOmzet.ToString("N2", culture),
                                profit.ToString("N2", culture),
                                totalTax.ToString("N2", culture),
                                true
                            );
                        }
                        
                    });

                    // ---------- FOOTER ----------
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                    });
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }
    }
}
