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
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.periode == month && y.isclosed == "Y").ToList();
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;

            // Render the "Index" view as a PDF

            if (dataclosing.Count > 0)
            {
                if (isyearly)
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000).ToList();
                    var datajpb = db.JpbTbl.Where(y =>  y.TransDate.Year == year).ToList();
                    var datajpn = db.JpnTbl.Where(y =>  y.TransDate.Year == year).ToList();
                    var datajm = db.JmTbl.Where(y =>  y.TransDate.Year == year).ToList();
                    obj.akundata = dataacc;
                    obj.jpbdata = datajpb;
                    obj.jpndata = datajpn;
                    obj.jmdata = datajm;
                    obj.closingdata = dataclosing;

                    List<LRRptModel> rptdata = new List<LRRptModel>();
                    
                    foreach (var dt in dataacc)
                    {
                        LRRptModel fld = new LRRptModel();
                        fld.akun = dt.account_no.ToString();
                        fld.description = dt.account_name;
                        if (dt.akundk == "K")
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                        }
                        else
                       
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);

                            fld.total = "("+ (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00")+")" ;
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;
                    byte[] pdfBytes = GeneratePdfV2(obj);
                    var FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                    return File(pdfBytes, "application/pdf", FileName);

                    //return new ViewAsPdf("LR_Rpt", obj)
                    //{
                    //    FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf",
                    //    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    //    PageMargins = new Rotativa.AspNetCore.Options.Margins
                    //    {
                    //        Left = 5,   // Set narrow margin for the left (in millimeters)
                    //        Right = 5,  // Set narrow margin for the right (in millimeters)
                    //        Top = 10,   // Set narrow margin for the top (in millimeters)
                    //        Bottom = 10 // Set narrow margin for the bottom (in millimeters)
                    //    }
                    //};
                }
                else
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000).ToList();
                    var datajpb = db.JpbTbl.Where(y => y.TransDate.Month == month && y.TransDate.Year == year).ToList();
                    var datajpn = db.JpnTbl.Where(y => y.TransDate.Month == month && y.TransDate.Year == year).ToList();
                    var datajm = db.JmTbl.Where(y => y.TransDate.Month == month && y.TransDate.Year == year).ToList();
                    obj.akundata = dataacc;
                    obj.jpbdata = datajpb;
                    obj.jpndata = datajpn;
                    obj.jmdata = datajm;
                    obj.closingdata = dataclosing;

                    List<LRRptModel> rptdata = new List<LRRptModel>();

                    foreach (var dt in dataacc)
                    {
                        LRRptModel fld = new LRRptModel();
                        fld.akun = dt.account_no.ToString();
                        fld.description = dt.account_name;
                        if (dt.akundk == "K")
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                        }
                        else
                        
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00")+ ")" ;
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;

                    byte[] pdfBytes = GeneratePdfV2(obj);
                    var FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                    return File(pdfBytes, "application/pdf", FileName);

                    //return new ViewAsPdf("LR_Rpt", obj)
                    //{
                    //    FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf",
                    //    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    //    PageMargins = new Rotativa.AspNetCore.Options.Margins
                    //    {
                    //        Left = 5,   // Set narrow margin for the left (in millimeters)
                    //        Right = 5,  // Set narrow margin for the right (in millimeters)
                    //        Top = 10,   // Set narrow margin for the top (in millimeters)
                    //        Bottom = 10 // Set narrow margin for the bottom (in millimeters)
                    //    }
                    //};
                }
               
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
                    page.Size(PageSizes.A4); // Change from A2 to A4 for better scaling
                    page.Margin(20);

                    page.Header().Column(col =>
                    {
                        col.Item().Text("Report Laba Rugi")
                            .Bold()
                            .FontSize(15)
                            .AlignCenter();
                        col.Item().Text("Tahun: " + obj.year)
                            .FontSize(10)
                            .AlignCenter();

                        if (!obj.isYearly)
                        {
                            col.Item().Text("Periode: " + obj.month)
                                .FontSize(10)
                                .AlignCenter();
                        }

                        col.Item().Text("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                            .FontSize(10)
                            .AlignCenter();
                        col.Item().Text("Company XYZ - Financial Report")
                            .FontSize(10)
                            .AlignCenter();
                    });

                    page.Content().Table(table =>
                    {
                        // Define Columns (Adjusted for better fit)
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1); // Account No
                            columns.RelativeColumn(3); // Description
                            columns.RelativeColumn(2); // Value
                            columns.RelativeColumn(2); // Total
                        });

                        // Table Header
                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(5).Text("Account No").Bold().FontSize(10);
                            header.Cell().Border(1).Padding(5).Text("Description").Bold().FontSize(10);
                            header.Cell().Border(1).Padding(5).Text("Value").Bold().FontSize(10);
                            header.Cell().Border(1).Padding(5).Text("Total").Bold().FontSize(10);
                        });

                        string previousFirstDigit = null;

                        foreach (var dt in obj.ReportModel)
                        {
                            string currentFirstDigit = dt.akun.Substring(0, 1);

                            // Add a subtotal row when a new category starts
                            if (previousFirstDigit != currentFirstDigit && previousFirstDigit != null)
                            {
                                var subtotal = obj.ReportModel.Where(y => y.akun.StartsWith(previousFirstDigit)).Sum(y => y.totalint);

                                table.Cell().Border(1).Padding(5).Text("").FontSize(8);
                                table.Cell().Border(1).Padding(5).Text("Sub Total").FontSize(8).Bold();
                                table.Cell().Border(1).Padding(5).Text("").FontSize(8);
                                table.Cell().Border(1).Padding(5).Text(subtotal.ToString("#,##0.00;(#,##0.00)")).FontSize(8);

                                if (previousFirstDigit == "5")
                                {
                                    string[] codearr = { "4", "5" };
                                    var subtotal2 = obj.ReportModel.Where(y => codearr.Contains(y.akun.Substring(0, 1))).Sum(y => y.totalint);

                                    table.Cell().Border(1).Padding(5).Text("").FontSize(8);
                                    table.Cell().Border(1).Padding(5).Text("HPP").FontSize(8).Bold();
                                    table.Cell().Border(1).Padding(5).Text("").FontSize(8);
                                    table.Cell().Border(1).Padding(5).Text(subtotal2.ToString("#,##0.00;(#,##0.00)")).FontSize(8);
                                }
                            }

                            // Data row
                            table.Cell().Border(1).Padding(5).Text(dt.akun).FontSize(8);
                            table.Cell().Border(1).Padding(5).Text(dt.description).FontSize(8);
                            table.Cell().Border(1).Padding(5).Text(dt.total != "(0,00)" ? dt.total : "").FontSize(8);
                            table.Cell().Border(1).Padding(5).Text("").FontSize(8);

                            previousFirstDigit = currentFirstDigit;
                        }

                        // Final Total Row
                        var subtotal3 = obj.ReportModel.Sum(y => y.totalint);
                        table.Cell().Border(1).Padding(5).Text("").FontSize(8);
                        table.Cell().Border(1).Padding(5).Text("Laba Bersih Usaha").FontSize(8).Bold();
                        table.Cell().Border(1).Padding(5).Text("").FontSize(8);
                        table.Cell().Border(1).Padding(5).Text(subtotal3.ToString("#,##0.00;(#,##0.00)")).FontSize(8);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Generated using QuestPDF").FontSize(8);
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }



        public IActionResult DownloadTablePdf()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            byte[] pdfBytes = GenerateTablePdf();

            return File(pdfBytes, "application/pdf", "table.pdf");
        }

        private byte[] GenerateTablePdf()
        {
            using var stream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.Header().Column(col =>
                    {
                        col.Item().Text("Report Laba Rugi")
                            .Bold()
                            .FontSize(15)
                            .AlignCenter();

                        col.Item().Text("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                            .FontSize(10)
                            .AlignCenter();

                        col.Item().Text("Company XYZ - Financial Report")
                            .FontSize(10)
                            .AlignCenter();
                    });

                    page.Content().Table(table =>
                    {
                        // Define Columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);  // ID column
                            columns.RelativeColumn();   // Name column
                            columns.RelativeColumn();   // Role column
                        });

                        // Header Row
                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(5).Text("ID").Bold();
                            header.Cell().Border(1).Padding(5).Text("Name").Bold();
                            header.Cell().Border(1).Padding(5).Text("Role").Bold();
                        });

                        // Data Rows
                        for (int i = 1; i <= 5; i++)
                        {
                            table.Cell().Border(1).Padding(5).Text(i.ToString());
                            table.Cell().Border(1).Padding(5).Text($"User {i}");
                            table.Cell().Border(1).Padding(5).Text("Developer");
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Generated using QuestPDF");
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }



    }
}
