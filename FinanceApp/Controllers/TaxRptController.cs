using BaseLineProject.Data;
using BaseLineProject.Models;
using FinanceApp.Models;
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

        private IHostingEnvironment Environment;
        private readonly ILogger<LRController> _logger;

        public TaxRptController(FormDBContext db, ILogger<TaxRptController> logger, IHostingEnvironment _environment)
        {
            logger = logger;
            Environment = _environment;
            this.db = db;

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
        public async Task<IActionResult> GeneratePdf([Bind] LRModel obj)
        {
            var year = obj.year;
            var isyearly = obj.isYearly;
            var month = obj.month;
            if (isyearly)
            {
                month = 12;
            }
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.periode >= 1 && y.periode <= month && y.isclosed == "Y").ToList();
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            // Render the "Index" view as a PDF

            if (dataclosing.Count == month)
            {
                obj.jpndata = db.JpnTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();

                byte[] pdfBytes = GeneratePdfV2(obj);
                var FileName = "TaxRpt" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);

            }
            else
            {
                return RedirectToAction("GeneratePdf", new { message = "Incorrect Periode Settings" });
            }

        }

        //private byte[] GeneratePdfV2(LRModel obj)
        //{
        //    var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
        //    var jpndata = obj.jpndata;
        //    var result = jpndata
        //   .GroupBy(d => new { d.TransDate.Year, d.TransDate.Month })
        //   .Select(g => new taxrptmodel
        //   {
        //       bulan = g.Key.Month.ToString(),
        //       value = g.Sum(d => d.Value),
        //       taxval = (Convert.ToDecimal(g.Sum(d => d.Value)) * Convert.ToDecimal(0.5)) / Convert.ToDecimal(100) // You can also count items here
        //   });
         
        //    using var stream = new MemoryStream();
        //    Document.Create(container =>
        //    {
        //        container.Page(page =>
        //        {
        //            page.Size(PageSizes.A4);
        //            page.Margin(10); // Reduced margin

        //            page.Header().Column(col =>
        //            {
        //                col.Item().Text("Report Preview Laporan")
        //                   .Bold()
        //                   .FontSize(12) // Reduced font size
        //                   .AlignCenter();

        //                col.Item().Text($"Tahun: {obj.year}")
        //                    .FontSize(8)
        //                    .AlignCenter();

        //                if (!obj.isYearly)
        //                {
        //                    col.Item().Text($"Periode: {obj.month}")
        //                        .FontSize(8)
        //                        .AlignCenter();
        //                }

        //                col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
        //                    .FontSize(8)
        //                    .AlignCenter();
        //                col.Item().Text("Company XYZ - Financial Report")
        //                    .FontSize(8)
        //                    .AlignCenter();
        //            });

        //            page.Content().Table(table =>
        //            {
        //                // Define just 2 columns
        //                table.ColumnsDefinition(columns =>
        //                {
        //                    columns.RelativeColumn(3); // Description
        //                    columns.RelativeColumn(1); // Value
        //                    columns.RelativeColumn(1); // Value

        //                });

        //                void AddRow(string desc, string value, bool bold = false)
        //                {
        //                    table.Cell().Border(1).Padding(2).Text(desc).FontSize(7).Bold();
        //                    table.Cell().Border(1).Padding(2).Text(value).FontSize(7).AlignRight().Bold();
        //                    table.Cell().Border(1).Padding(2).Text(value).FontSize(7).AlignRight().Bold();

        //                }

        //                // Header
        //                AddRow("Bulan", "Peredaran Bruto","Potongan Pajak", bold: true);

        //                AddRow("Januari", SaldoLaba.ToString("#,##0.00;(#,##0.00)"));

        //            });

        //            page.Footer()
        //                .AlignCenter()
        //                .Text("Generated using QuestPDF").FontSize(7); // Smaller footer font
        //        });
        //    }).GeneratePdf(stream);

        //    return stream.ToArray();
        //}
        private byte[] GeneratePdfV2(LRModel obj)
        {
            // 1. PREPARE DATA
            // We use CultureInfo for Indonesian month names (Januari, Februari) and number formatting
            var culture = new System.Globalization.CultureInfo("id-ID");

            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            var jpndata = obj.jpndata;
            var result = jpndata
               .GroupBy(d => new { d.TransDate.Year, d.TransDate.Month })
               .Select(g => new taxrptmodel
               {
                   bulan = g.Key.Month.ToString(),
                   value = g.Sum(d => d.Value),
                   taxval = (Convert.ToDecimal(g.Sum(d => d.Value)) * Convert.ToDecimal(0.5)) / Convert.ToDecimal(100) // You can also count items here
               });

            // 2. GENERATE PDF
            using var stream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // --- HEADER ---
                    page.Header().Column(col =>
                    {
                        col.Item().Text("Report Preview Laporan pajak")
                           .Bold().FontSize(12).AlignCenter();

                        col.Item().Text($"Tahun: {obj.year}").FontSize(9).AlignCenter();

                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(8).FontColor(Colors.Grey.Medium).AlignCenter();

                        col.Spacing(10); // Add space after header
                    });

                    // --- CONTENT TABLE ---
                    page.Content().Table(table =>
                    {
                        // Define 3 columns: 
                        // Col 1 (Month) is wider. Col 2 & 3 (Values) are equal width.
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Bulan
                            columns.RelativeColumn(1.5f); // Peredaran Bruto
                            columns.RelativeColumn(1.5f); // Potongan Pajak
                        });

                        // Helper function to create consistent cells
                        void AddRow(string col1Text, string col2Text, string col3Text, bool isHeader = false)
                        {
                            var fontSize = isHeader ? 8 : 7;
                            var fontWeight = isHeader ? FontWeight.Bold : FontWeight.Normal;
                            var bgColor = isHeader ? Colors.Grey.Lighten3 : Colors.White;

                            // Column 1: Bulan (Align Left)
                            table.Cell().Border(1).Background(bgColor).Padding(4)
                                .Text(col1Text).FontSize(fontSize);

                            // Column 2: Value (Align Right)
                            table.Cell().Border(1).Background(bgColor).Padding(4)
                                .Text(col2Text).FontSize(fontSize).AlignRight();

                            // Column 3: Tax (Align Right)
                            table.Cell().Border(1).Background(bgColor).Padding(4)
                                .Text(col3Text).FontSize(fontSize).AlignRight();
                        }

                        // A. Table Header
                        AddRow("Bulan", "Peredaran Bruto", "Potongan Pajak", isHeader: true);

                        // B. Data Loop
                        foreach (var item in result)
                        {
                            // Convert Month Number (1) to Name ("Januari")
                            string monthName = item.bulan;

                            // Format Numbers (N2 adds commas and 2 decimal places)
                            string formattedValue = item.value.ToString("N2", culture);
                            string formattedTax = item.taxval.ToString("N2", culture);

                            AddRow(monthName, formattedValue, formattedTax);
                        }

                        // C. Grand Total (Optional - Good for reports)
                        var totalBruto = result.Sum(x => x.value);
                        var totalTax = result.Sum(x => x.taxval);

                        AddRow("TOTAL",
                               totalBruto.ToString("N2", culture),
                               totalTax.ToString("N2", culture),
                               isHeader: true);
                    });

                    // --- FOOTER ---
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
