using System;
using System.Collections.Generic;
using System.Linq;
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
using BaseLineProject.Data;
using FinanceApp.Models;
using BaseLineProject.Models;
using System.Text;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using System.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Net;

namespace FinanceApp.Controllers
{
    public class LRController : Controller
    {
        private readonly FormDBContext db;
        public const string SessionKeyName = "TransDateStr";

        private IHostingEnvironment Environment;
        private readonly ILogger<LRController> _logger;

        public LRController(FormDBContext db, ILogger<LRController> logger, IHostingEnvironment _environment)
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
        public IActionResult GeneratePreview(string message)
        {
            ViewBag.Message = message;
            return View();
        }
        public IActionResult LR_Rpt(LRModel datas)
        {
            // Pass any necessary data to the view via the ViewData or ViewBag
            
            ViewData["ReportTitle"] = "Sample Report";
            

            return View(datas);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePdf([Bind] LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.periode >= 1 && y.periode <= month && y.isclosed == "Y").ToList();
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            // Render the "Index" view as a PDF

            if (dataclosing.Count == month)
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var datajpb = db.JpbTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                obj.akundata = dataacc;
                obj.jpbdata = datajpb;
                obj.jpndata = datajpn;
                obj.jmdata = datajm;
                obj.closingdata = dataclosing;
                obj.ispreview = false;

                List<LRRptModel> rptdata = new List<LRRptModel>();

                foreach (var dt in dataacc)
                {
                    LRRptModel fld = new LRRptModel();
                    fld.akun = dt.account_no.ToString();
                    fld.description = dt.account_name;
                    fld.akundk = dt.akundk;

                    if (dt.akundk == "K")
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        var totaljpbreversal = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) * -1;
                        var totaljpnreversal = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) * -1;
                        var totaljmreversal = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Credit) * -1;
                        var totaljpndiscreversal = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) * -1;
                        var totaljpbdiscreversal = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) * -1;

                        var totaljpb_c = datajpb.Where(y => y.Akun_Credit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value);
                        var totaljpn_c = datajpn.Where(y => y.Akun_Credit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value);
                        var totaljm_c = datajm.Where(y => y.Akun_Credit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Credit);
                        var totaljpndisc_c = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc_c = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc);

                        var totaljpbreversal_c = datajpb.Where(y => y.Akun_Debit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value) * -1;
                        var totaljpnreversal_c = datajpn.Where(y => y.Akun_Debit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value) * -1;
                        var totaljmreversal_c = datajm.Where(y => y.Akun_Debit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Credit) * -1;
                        var totaljpndiscreversal_c = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc) * -1;
                        var totaljpbdiscreversal_c = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc) * -1;

                        fld.total_current = (totaljpb_c + totaljpn_c + totaljm_c + totaljpndisc_c + totaljpbdisc_c + totaljpbdiscreversal_c + totaljpnreversal_c + totaljmreversal_c + totaljpndiscreversal_c + totaljpbdiscreversal_c).ToString("#,##0.00");
                        fld.totalint_current = totaljpb_c + totaljpn_c + totaljm_c + totaljpndisc_c + totaljpbdisc_c + totaljpbdiscreversal_c + totaljpnreversal_c + totaljmreversal_c + totaljpndiscreversal_c + totaljpbdiscreversal_c;


                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal).ToString("#,##0.00");
                        fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal;


                    }
                    else
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        var totaljpbreversal = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) * -1;
                        var totaljpnreversal = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) * -1;
                        var totaljmreversal = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) * -1;
                        var totaljpndiscreversal = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) * -1;
                        var totaljpbdiscreversal = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) * -1;

                        var totaljpb_c = datajpb.Where(y => y.Akun_Debit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value);
                        var totaljpn_c = datajpn.Where(y => y.Akun_Debit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value);
                        var totaljm_c = datajm.Where(y => y.Akun_Debit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Debit);
                        var totaljpndisc_c = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc_c = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc);

                        var totaljpbreversal_c = datajpb.Where(y => y.Akun_Credit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value) * -1;
                        var totaljpnreversal_c = datajpn.Where(y => y.Akun_Credit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value) * -1;
                        var totaljmreversal_c = datajm.Where(y => y.Akun_Credit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Credit) * -1;
                        var totaljpndiscreversal_c = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc) * -1;
                        var totaljpbdiscreversal_c = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc) * -1;

                        fld.total_current = "(" + (totaljpb_c + totaljpn_c + totaljm_c + totaljpndisc_c + totaljpbdisc_c + totaljpbdiscreversal_c + totaljpnreversal_c + totaljmreversal_c + totaljpndiscreversal_c + totaljpbdiscreversal_c).ToString("#,##0.00") + ")";
                        fld.totalint_current = -1 * (totaljpb_c + totaljpn_c + totaljm_c + totaljpndisc_c + totaljpbdisc_c + totaljpbdiscreversal_c + totaljpnreversal_c + totaljmreversal_c + totaljpndiscreversal_c + totaljpbdiscreversal_c);


                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;

                byte[] pdfBytes = GeneratePdfV3(obj);
                var FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);


            }
            else
            {
                return RedirectToAction("GeneratePdf", new { message = "Incorrect Periode Settings" });
            }

        }

        private byte[] GeneratePdfV2(LRModel obj)
        {
            using var stream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(10); // Reduced margin

                    page.Header().Column(col =>
                    {
                        if(obj.ispreview == true)
                        {
                            col.Item().Text("Report Laba Rugi (PREVIEW)")
                           .Bold()
                           .FontSize(12) // Reduced font size
                           .AlignCenter();
                        }
                        else
                        {
                            col.Item().Text("Report Laba Rugi")
                           .Bold()
                           .FontSize(12) // Reduced font size
                           .AlignCenter();
                        }
                       
                        col.Item().Text($"Tahun: {obj.year}")
                            .FontSize(8)
                            .AlignCenter();

                        if (!obj.isYearly)
                        {
                            col.Item().Text($"Periode: {obj.month}")
                                .FontSize(8)
                                .AlignCenter();
                        }

                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(8)
                            .AlignCenter();
                        col.Item().Text("Company XYZ - Financial Report")
                            .FontSize(8)
                            .AlignCenter();
                    });
                   
                    page.Content().Table(table =>
                    {
                        // Adjusted column widths
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1.5f); // Reduced Value column width
                            columns.RelativeColumn(1.5f); // Reduced Total column width
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(2).Text("Account No").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Description").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Value").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Total").Bold().FontSize(8);
                        });

                        string previousFirstDigit = null;

                        foreach (var dt in obj.ReportModel)
                        {
                            string currentFirstDigit = dt.akun.Substring(0, 1);

                            // Add a subtotal row when a new category starts
                            if (previousFirstDigit != currentFirstDigit && previousFirstDigit != null)
                            {
                                var subtotal = obj.ReportModel.Where(y => y.akun.StartsWith(previousFirstDigit)).Sum(y => y.totalint);

                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text("Sub Total").FontSize(7).Bold();
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text(subtotal.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();

                                if (previousFirstDigit == "5")
                                {
                                    string[] codearr = { "4", "5" };
                                    var subtotal2 = obj.ReportModel.Where(y => codearr.Contains(y.akun.Substring(0, 1))).Sum(y => y.totalint);

                                    table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                    table.Cell().Border(1).Padding(2).Text("HPP").FontSize(7).Bold();
                                    table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                    table.Cell().Border(1).Padding(2).Text(subtotal2.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();
                                }
                            }

                            table.Cell().Border(1).Padding(2).Text(dt.akun).FontSize(7);
                            table.Cell().Border(1).Padding(2).Text(dt.description).FontSize(7);
                            table.Cell().Border(1).Padding(2).Text(dt.akundk != "-" ? dt.total : "").FontSize(7).AlignRight();
                            table.Cell().Border(1).Padding(2).Text("").FontSize(7);

                            previousFirstDigit = currentFirstDigit;
                        }

                        var subtotal3 = obj.ReportModel.Sum(y => y.totalint);
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("Laba Bersih Usaha").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text(subtotal3.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Generated using QuestPDF").FontSize(7); // Smaller footer font
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }

        private byte[] GeneratePdfV3(LRModel obj)
        {
            using var stream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(10); // Reduced margin

                    page.Header().Column(col =>
                    {
                        if (obj.ispreview == true)
                        {
                            col.Item().Text("Report Laba Rugi (PREVIEW)")
                           .Bold()
                           .FontSize(12) // Reduced font size
                           .AlignCenter();
                        }
                        else
                        {
                            col.Item().Text("Report Laba Rugi")
                           .Bold()
                           .FontSize(12) // Reduced font size
                           .AlignCenter();
                        }

                        col.Item().Text($"Tahun: {obj.year}")
                            .FontSize(8)
                            .AlignCenter();

                        if (!obj.isYearly)
                        {
                            col.Item().Text($"Periode: {obj.month}")
                                .FontSize(8)
                                .AlignCenter();
                        }

                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(8)
                            .AlignCenter();
                        col.Item().Text("Company XYZ - Financial Report")
                            .FontSize(8)
                            .AlignCenter();
                    });

                    page.Content().Table(table =>
                    {
                        // Adjusted column widths
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1.5f); // Reduced Value column width
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f); // Reduced Value column width
                            columns.RelativeColumn(1.5f); // Reduced Total column width
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(2).Text("Account No").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Description").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Current").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Total Current").Bold().FontSize(8);

                            header.Cell().Border(1).Padding(2).Text("YTD").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Total YTD").Bold().FontSize(8);
                        });

                        string previousFirstDigit = null;

                        foreach (var dt in obj.ReportModel)
                        {
                            string currentFirstDigit = dt.akun.Substring(0, 1);

                            // Add a subtotal row when a new category starts
                            if (previousFirstDigit != currentFirstDigit && previousFirstDigit != null)
                            {
                                var subtotal = obj.ReportModel.Where(y => y.akun.StartsWith(previousFirstDigit)).Sum(y => y.totalint);
                                var subtotal_current = obj.ReportModel.Where(y => y.akun.StartsWith(previousFirstDigit)).Sum(y => y.totalint_current);

                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text("Sub Total").FontSize(7).Bold();
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text(subtotal_current.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text(subtotal.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();

                                if (previousFirstDigit == "5")
                                {
                                    string[] codearr = { "4", "5" };
                                    var subtotal2 = obj.ReportModel.Where(y => codearr.Contains(y.akun.Substring(0, 1))).Sum(y => y.totalint);
                                    var subtotal2_c = obj.ReportModel.Where(y => codearr.Contains(y.akun.Substring(0, 1))).Sum(y => y.totalint_current);

                                    table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                    table.Cell().Border(1).Padding(2).Text("HPP").FontSize(7).Bold();
                                    table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                    table.Cell().Border(1).Padding(2).Text(subtotal2_c.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();
                                    table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                    table.Cell().Border(1).Padding(2).Text(subtotal2.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();
                                }
                            }

                            table.Cell().Border(1).Padding(2).Text(dt.akun).FontSize(7);
                            table.Cell().Border(1).Padding(2).Text(dt.description).FontSize(7);
                            table.Cell().Border(1).Padding(2).Text(dt.akundk != "-" ? dt.total_current : "").FontSize(7).AlignRight();
                            table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                            table.Cell().Border(1).Padding(2).Text(dt.akundk != "-" ? dt.total : "").FontSize(7).AlignRight();
                            table.Cell().Border(1).Padding(2).Text("").FontSize(7);

                            previousFirstDigit = currentFirstDigit;
                        }

                        var subtotal3 = obj.ReportModel.Sum(y => y.totalint);
                        var subtotal3_c = obj.ReportModel.Sum(y => y.totalint_current);

                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("Laba Bersih Usaha").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text(subtotal3_c.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text(subtotal3.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Generated using QuestPDF").FontSize(7); // Smaller footer font
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }



        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePreview([Bind] LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.periode >= 1 && y.periode <= month && y.isclosed == "Y").ToList();
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
            var datajpb = db.JpbTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
            var datajpn = db.JpnTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
            var datajm = db.JmTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
            obj.akundata = dataacc;
            obj.jpbdata = datajpb;
            obj.jpndata = datajpn;
            obj.jmdata = datajm;
            obj.closingdata = dataclosing;
            obj.ispreview = false;

            List<LRRptModel> rptdata = new List<LRRptModel>();

            foreach (var dt in dataacc)
            {
                LRRptModel fld = new LRRptModel();
                fld.akun = dt.account_no.ToString();
                fld.description = dt.account_name;
                fld.akundk = dt.akundk;

                if (dt.akundk == "K")
                {
                    var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                    var totaljpbreversal = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) * -1;
                    var totaljpnreversal = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) * -1;
                    var totaljmreversal = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Credit) * -1;
                    var totaljpndiscreversal = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) * -1;
                    var totaljpbdiscreversal = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) * -1;

                    var totaljpb_c = datajpb.Where(y => y.Akun_Credit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value);
                    var totaljpn_c = datajpn.Where(y => y.Akun_Credit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value);
                    var totaljm_c = datajm.Where(y => y.Akun_Credit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Credit);
                    var totaljpndisc_c = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc_c = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc);

                    var totaljpbreversal_c = datajpb.Where(y => y.Akun_Debit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value) * -1;
                    var totaljpnreversal_c = datajpn.Where(y => y.Akun_Debit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value) * -1;
                    var totaljmreversal_c = datajm.Where(y => y.Akun_Debit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Credit) * -1;
                    var totaljpndiscreversal_c = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc) * -1;
                    var totaljpbdiscreversal_c = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc) * -1;

                    fld.total_current = (totaljpb_c + totaljpn_c + totaljm_c + totaljpndisc_c + totaljpbdisc_c + totaljpbdiscreversal_c + totaljpnreversal_c + totaljmreversal_c + totaljpndiscreversal_c + totaljpbdiscreversal_c).ToString("#,##0.00");
                    fld.totalint_current = totaljpb_c + totaljpn_c + totaljm_c + totaljpndisc_c + totaljpbdisc_c + totaljpbdiscreversal_c + totaljpnreversal_c + totaljmreversal_c + totaljpndiscreversal_c + totaljpbdiscreversal_c;


                    fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal).ToString("#,##0.00");
                    fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal;


                }
                else
                {
                    var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                    var totaljpbreversal = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) * -1;
                    var totaljpnreversal = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) * -1;
                    var totaljmreversal = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) * -1;
                    var totaljpndiscreversal = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) * -1;
                    var totaljpbdiscreversal = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) * -1;

                    var totaljpb_c = datajpb.Where(y => y.Akun_Debit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value);
                    var totaljpn_c = datajpn.Where(y => y.Akun_Debit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value);
                    var totaljm_c = datajm.Where(y => y.Akun_Debit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Debit);
                    var totaljpndisc_c = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc_c = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc);

                    var totaljpbreversal_c = datajpb.Where(y => y.Akun_Credit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value) * -1;
                    var totaljpnreversal_c = datajpn.Where(y => y.Akun_Credit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value) * -1;
                    var totaljmreversal_c = datajm.Where(y => y.Akun_Credit == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Credit) * -1;
                    var totaljpndiscreversal_c = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc) * -1;
                    var totaljpbdiscreversal_c = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no && y.TransDate.Month == month).Sum(y => (long)y.Value_Disc) * -1;

                    fld.total_current = "(" + (totaljpb_c + totaljpn_c + totaljm_c + totaljpndisc_c + totaljpbdisc_c + totaljpbdiscreversal_c + totaljpnreversal_c + totaljmreversal_c + totaljpndiscreversal_c + totaljpbdiscreversal_c).ToString("#,##0.00") + ")";
                    fld.totalint_current = -1 * (totaljpb_c + totaljpn_c + totaljm_c + totaljpndisc_c + totaljpbdisc_c + totaljpbdiscreversal_c + totaljpnreversal_c + totaljmreversal_c + totaljpndiscreversal_c + totaljpbdiscreversal_c);


                    fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal).ToString("#,##0.00") + ")";
                    fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal);

                }


                rptdata.Add(fld);
            }
            obj.ReportModel = rptdata;

            byte[] pdfBytes = GeneratePdfV3(obj);
            var FileName = "LabaRugiPreview" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
            return File(pdfBytes, "application/pdf", FileName);


        }







    }
}
