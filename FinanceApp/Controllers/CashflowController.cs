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
using Syncfusion.Drawing;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Database;

namespace FinanceApp.Controllers
{
    public class CashflowController : Controller
    {
        private readonly FormDBContext db;
        public const string SessionKeyName = "TransDateStr";

        private IHostingEnvironment Environment;
        private readonly ILogger<LRController> _logger;

        public CashflowController(FormDBContext db, ILogger<CashflowController> logger, IHostingEnvironment _environment)
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
            var datatb = getdatatbclosed(obj);
            if (dataclosing.Count == month)
            {
                if (isyearly)
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpb = db.JpbTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpn = db.JpnTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajm = db.JmTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
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


                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;
                    obj.TB_D = datatb.TB_D;
                    obj.TB_K = datatb.TB_K;
                    byte[] pdfBytes = GeneratePdfV2(obj);
                    var FileName = "Cashflow" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
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


                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;
                    obj.TB_D = datatb.TB_D;
                    obj.TB_K = datatb.TB_K;
                    byte[] pdfBytes = GeneratePdfV2(obj);
                    var FileName = "CashflowYtd" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
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

        public LRModel getdatatbclosed(LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.isclosed == "N" && y.periode == month || String.IsNullOrEmpty(y.isclosed)).ToList();
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            // Render the "Index" view as a PDF

            if (dataclosing.Count == 0)
            {
                if (isyearly)
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();


                    var jpballdt = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejpb = jpballdt.TransDate.Year;

                    var jpnalldt = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejpn = jpnalldt.TransDate.Year;

                    var jmalldt = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejm = jmalldt.TransDate.Year;




                    var datajpb = db.JpbTbl.Where(y => y.TransDate.Year >= startdatejpb && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpn = db.JpnTbl.Where(y => y.TransDate.Year >= startdatejpn && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajm = db.JmTbl.Where(y => y.TransDate.Year >= startdatejm && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();

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


                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;
                    List<TBModel> TBData_D = new List<TBModel>();
                    foreach (var dt in dataacc_D)
                    {
                        TBModel fld = new TBModel();
                        fld.AccountNo_D = dt.account_no;
                        fld.Desc_D = dt.account_name;
                        fld.akundk_D = dt.akundk;
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                        {
                            fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                            fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }
                        else
                        {
                            fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                        }
                        TBData_D.Add(fld);



                    }

                    List<TBModel> TBData_K = new List<TBModel>();
                    foreach (var dt in dataacc_K)
                    {
                        TBModel fld = new TBModel();
                        fld.AccountNo_K = dt.account_no;
                        fld.Desc_K = dt.account_name;
                        fld.akundk_K = dt.akundk;
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);


                        fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                        TBData_K.Add(fld);



                    }
                    obj.TB_D = TBData_D;
                    obj.TB_K = TBData_K;
                    
                }
                else
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();

                    var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();

                    var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();


                    var jpballdt = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejpb = jpballdt.TransDate.Year;
                    var startmonthjpb = jpballdt.TransDate.Month;

                    var jpnalldt = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejpn = jpnalldt.TransDate.Year;
                    var startmonthjpn = jpnalldt.TransDate.Month;

                    var jmalldt = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejm = jmalldt.TransDate.Year;
                    var startmonthjm = jmalldt.TransDate.Month;




                    var datajpb = db.JpbTbl.Where(y => (y.TransDate.Year > startdatejpb ||
                                     (y.TransDate.Year == startdatejpb && y.TransDate.Month >= startmonthjpb))
                                    &&
                                    (y.TransDate.Year < year ||
                                     (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpn = db.JpnTbl.Where(y => (y.TransDate.Year > startdatejpn ||
                                     (y.TransDate.Year == startdatejpn && y.TransDate.Month >= startmonthjpn))
                                    &&
                                    (y.TransDate.Year < year ||
                                     (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();
                    var datajm = db.JmTbl.Where(y => (y.TransDate.Year > startdatejm ||
                                     (y.TransDate.Year == startdatejm && y.TransDate.Month >= startmonthjm))
                                    &&
                                    (y.TransDate.Year < year ||
                                     (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();

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


                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;
                    List<TBModel> TBData_D = new List<TBModel>();
                    foreach (var dt in dataacc_D)
                    {
                        TBModel fld = new TBModel();
                        fld.AccountNo_D = dt.account_no;
                        fld.Desc_D = dt.account_name;
                        fld.akundk_D = dt.akundk;
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                        {
                            fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                            fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }
                        else
                        {
                            fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                        }
                        TBData_D.Add(fld);



                    }

                    List<TBModel> TBData_K = new List<TBModel>();
                    foreach (var dt in dataacc_K)
                    {
                        TBModel fld = new TBModel();
                        fld.AccountNo_K = dt.account_no;
                        fld.Desc_K = dt.account_name;
                        fld.akundk_K = dt.akundk;
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);


                        fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                        TBData_K.Add(fld);



                    }
                    obj.TB_D = TBData_D;
                    obj.TB_K = TBData_K;
                    
                }

            }
            
            return obj;
        }

        public LRModel getdatatbpreview(LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            // Render the "Index" view as a PDF
            if (isyearly)
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
                var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();


                var jpballdt = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejpb = jpballdt.TransDate.Year;

                var jpnalldt = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejpn = jpnalldt.TransDate.Year;

                var jmalldt = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejm = jmalldt.TransDate.Year;




                var datajpb = db.JpbTbl.Where(y => y.TransDate.Year >= startdatejpb && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => y.TransDate.Year >= startdatejpn && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => y.TransDate.Year >= startdatejm && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();

                obj.akundata = dataacc;
                obj.jpbdata = datajpb;
                obj.jpndata = datajpn;
                obj.jmdata = datajm;
                obj.ispreview = true;
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


                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                List<TBModel> TBData_D = new List<TBModel>();
                foreach (var dt in dataacc_D)
                {
                    TBModel fld = new TBModel();
                    fld.AccountNo_D = dt.account_no;
                    fld.Desc_D = dt.account_name;
                    fld.akundk_D = dt.akundk;
                    var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                    {
                        fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                        fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }
                    else
                    {
                        fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                    }
                    TBData_D.Add(fld);



                }

                List<TBModel> TBData_K = new List<TBModel>();
                foreach (var dt in dataacc_K)
                {
                    TBModel fld = new TBModel();
                    fld.AccountNo_K = dt.account_no;
                    fld.Desc_K = dt.account_name;
                    fld.akundk_K = dt.akundk;
                    var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);


                    fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                    fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                    TBData_K.Add(fld);



                }
                obj.TB_D = TBData_D;
                obj.TB_K = TBData_K;
            }
            else
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();

                var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();

                var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
                var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();


                var jpballdt = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejpb = jpballdt.TransDate.Year;
                var startmonthjpb = jpballdt.TransDate.Month;

                var jpnalldt = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejpn = jpnalldt.TransDate.Year;
                var startmonthjpn = jpnalldt.TransDate.Month;

                var jmalldt = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejm = jmalldt.TransDate.Year;
                var startmonthjm = jmalldt.TransDate.Month;




                var datajpb = db.JpbTbl.Where(y => (y.TransDate.Year > startdatejpb ||
                                 (y.TransDate.Year == startdatejpb && y.TransDate.Month >= startmonthjpb))
                                &&
                                (y.TransDate.Year < year ||
                                 (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => (y.TransDate.Year > startdatejpn ||
                                 (y.TransDate.Year == startdatejpn && y.TransDate.Month >= startmonthjpn))
                                &&
                                (y.TransDate.Year < year ||
                                 (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => (y.TransDate.Year > startdatejm ||
                                 (y.TransDate.Year == startdatejm && y.TransDate.Month >= startmonthjm))
                                &&
                                (y.TransDate.Year < year ||
                                 (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();

                obj.akundata = dataacc;
                obj.jpbdata = datajpb;
                obj.jpndata = datajpn;
                obj.jmdata = datajm;
                obj.ispreview = true;

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


                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                List<TBModel> TBData_D = new List<TBModel>();
                foreach (var dt in dataacc_D)
                {
                    TBModel fld = new TBModel();
                    fld.AccountNo_D = dt.account_no;
                    fld.Desc_D = dt.account_name;
                    fld.akundk_D = dt.akundk;
                    var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                    {
                        fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                        fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }
                    else
                    {
                        fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                    }
                    TBData_D.Add(fld);



                }

                List<TBModel> TBData_K = new List<TBModel>();
                foreach (var dt in dataacc_K)
                {
                    TBModel fld = new TBModel();
                    fld.AccountNo_K = dt.account_no;
                    fld.Desc_K = dt.account_name;
                    fld.akundk_K = dt.akundk;
                    var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);


                    fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                    fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                    TBData_K.Add(fld);



                }
                obj.TB_D = TBData_D;
                obj.TB_K = TBData_K;
            }
            return obj;
        }

        private byte[] GeneratePdfV2(LRModel obj)
        {
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            var datalastyear = db.ClosingValueTbl.Where(y => y.company_id == datas.COMPANY_ID && y.year == obj.year - 1).ToList();
            int[] accpenyusutan = new int[] { 1200006, 1200007, 1200008, 1200009 };
            int[] accpiutang = new int[] { 1100006, 1100009, 1100012, 1100013, 1100014 };
            int[] acchutang = new int[] { 2100001, 2100002, 2100003, 2200001, 2200002, 2300001, 2300002, 2300003, 2300004, 2300005, };
            int[] accperalatan = new int[] { 1200003, 1200005 };

            var kasly = (long)0;
            if(datalastyear.Count() != 0)
            {
                var datalrly = datalastyear.Where(y => y.src == "LR").ToList();
                var dataTB_Dly = datalastyear.Where(y => y.src == "TB_D").ToList();
                var dataTB_Kly = datalastyear.Where(y => y.src == "TB_K").ToList();
                var creditly = datalrly.Sum(y => (long)y.credit);
                var debitly = datalrly.Sum(y => (long)y.debit);
                var saldolabaly = creditly + debitly;
                var penyusutanly = dataTB_Dly.Where(y => accpenyusutan.Contains(y.Akun_Debit)).Sum(y => (long)y.debit) * -1;
                var piutangly = dataTB_Dly.Where(y => accpiutang.Contains(y.Akun_Debit)).Sum(y => (long)y.debit) * -1;
                var hutangly = dataTB_Kly.Where(y => acchutang.Contains(y.Akun_Debit)).Sum(y => (long)y.debit);
                var kasbersihoperasily = saldolabaly + penyusutanly + piutangly + hutangly;
                var datapembelianperalatanly = datalastyear.Where(y => accperalatan.Contains(y.Akun_Debit)).Sum(y => (long)y.debit);
                var pembelianperalatannegately = datapembelianperalatanly * -1;
                var modally = dataTB_Kly.Where(y => y.Akun_Debit == 3000001).FirstOrDefault().debit;
                var kasbersihinventasily = datapembelianperalatanly + modally;
                kasly = kasbersihoperasily + pembelianperalatannegately + kasbersihinventasily;




            }

            var SaldoLaba = obj.ReportModel.Sum(y => y.totalint);
            var penyusutan = obj.TB_D.Where(y => accpenyusutan.Contains(y.AccountNo_D)).Sum(y => (long)y.Value_D_int) * -1;
            var piutang = obj.TB_D.Where(y => accpiutang.Contains(y.AccountNo_D)).Sum(y => (long)y.Value_D_int) * -1;
            var hutang = obj.TB_K.Where(y => acchutang.Contains(y.AccountNo_K)).Sum(y => (long)y.Value_K_int);
            var test = SaldoLaba + piutang + hutang;
            var kasbersihoperasi = SaldoLaba + penyusutan + piutang+ hutang;
            var datapembelianperalatan = obj.jmdata.Where(y => accperalatan.Contains(y.Akun_Debit)).Sum(y => y.Debit);
            var pembelianperalatannegate = datapembelianperalatan * -1;
            var modal = obj.TB_K.Where(y => y.AccountNo_K == 3000001).FirstOrDefault().Value_K_int;
            var kasbersihinventasi = datapembelianperalatan + modal;
            var penambahankas = kasbersihoperasi + pembelianperalatannegate + kasbersihinventasi;

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
                            col.Item().Text("Report Arus Kas(PREVIEW)")
                           .Bold()
                           .FontSize(12) // Reduced font size
                           .AlignCenter();
                        }
                        else
                        {
                            col.Item().Text("Report Arus Kas")
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
                        // Define just 2 columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Description
                            columns.RelativeColumn(1); // Value
                        });

                        void AddRow(string desc, string value, bool bold = false)
                        {
                            table.Cell().Border(1).Padding(2).Text(desc).FontSize(7).Bold();
                            table.Cell().Border(1).Padding(2).Text(value).FontSize(7).AlignRight().Bold();
                        }

                        // Header
                        AddRow("Description", "Value", bold: true);

                        // Section: Operasi
                        AddRow("Arus Kas dari Aktivitas Operasi", "", bold: true);
                        AddRow("Saldo Laba", SaldoLaba.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Penyusutan", penyusutan.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Kenaikan Piutang", piutang.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Kenaikan Hutang", hutang.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Kas Bersih dari Aktivitas Operasi", kasbersihoperasi.ToString("#,##0.00;(#,##0.00)"), bold: true);

                        // Section: Investasi
                        AddRow("Arus Kas dari Aktivitas Investasi", "", bold: true);
                        AddRow("Penambahan Aset Tetap", pembelianperalatannegate.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Kas Bersih dari Aktivitas Investasi", pembelianperalatannegate.ToString("#,##0.00;(#,##0.00)"), bold: true);

                        // Section: Pembiayaan
                        AddRow("Arus Kas dari Aktivitas Pembiayaan", "", bold: true);
                        AddRow("Penambahan Aset Tetap", datapembelianperalatan.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Modal", modal.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Kas Bersih dari Aktivitas Pembiayaan", kasbersihinventasi.ToString("#,##0.00;(#,##0.00)"), bold: true);

                        // Section: Summary
                        AddRow("Penambahan Bersih Kas dan Bank", penambahankas.ToString("#,##0.00;(#,##0.00)"), bold: true);
                        AddRow("Kas dan Bank Awal Tahun", kasly.ToString("#,##0.00;(#,##0.00)"), bold: true); // Add value if available
                        AddRow("Kas dan Bank Akhir Tahun", penambahankas.ToString("#,##0.00;(#,##0.00)"), bold: true);
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
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.periode == month && y.isclosed == "Y").ToList();
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            var datatb = getdatatbpreview(obj);
            // Render the "Index" view as a PDF

            if (isyearly)
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var datajpb = db.JpbTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
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


                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                obj.TB_D = datatb.TB_D;
                obj.TB_K = datatb.TB_K;
                obj.ispreview = true;
                byte[] pdfBytes = GeneratePdfV2(obj);
                var FileName = "Cashflowpreview" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);


            }
            else
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


                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc + totaljpbdiscreversal + totaljpnreversal + totaljmreversal + totaljpndiscreversal + totaljpbdiscreversal);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                obj.TB_D = datatb.TB_D;
                obj.TB_K = datatb.TB_K;
                byte[] pdfBytes = GeneratePdfV2(obj);
                var FileName = "CashflowPreviewYtd" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);

            }

        }

    }
}
