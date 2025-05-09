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
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                        }
                        else

                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

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
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                        }
                        else

                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

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
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                        }
                        else
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

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
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                        }
                        else
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

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
                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                    }
                    else
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

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
                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                    }
                    else
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

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
            var SaldoLaba = obj.ReportModel.Sum(y => y.totalint);
            int[] accpenyusutan = new int[] { 1200006, 1200007, 1200008, 1200009 };

            var penyusutan = obj.TB_D.Where(y => accpenyusutan.Contains(y.AccountNo_D)).Sum(y => (long)y.Value_D_int) * -1;
            int[] accpiutang = new int[] { 1100006, 1100009, 1100012, 1100013, 1100014 };
            var piutang = obj.TB_D.Where(y => accpiutang.Contains(y.AccountNo_D)).Sum(y => (long)y.Value_D_int) * -1;
            int[] acchutang = new int[] { 2100001, 2100002, 2100003, 2200001, 2200002, 2300001, 2300002, 2300003, 2300004, 2300005, };
            var hutang = obj.TB_K.Where(y => acchutang.Contains(y.AccountNo_K)).Sum(y => (long)y.Value_K_int);
            var kasbersihoperasi = SaldoLaba + penyusutan + piutang+ hutang;
            int[] accperalatan = new int[] { 1200003, 1200005 };
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
                            header.Cell().Border(1).Padding(2).Text("Description").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Value").Bold().FontSize(8);
                        });

                        table.Cell().Border(1).Padding(2).Text("Arus Kas dari Aktivitas Operasi").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("Saldo Laba").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text(SaldoLaba.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();

                        table.Cell().Border(1).Padding(2).Text("Penyusutan").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text(penyusutan.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();

                        table.Cell().Border(1).Padding(2).Text("Kenaikan piutang").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text(piutang.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();

                        table.Cell().Border(1).Padding(2).Text("Kenaikan hutang").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text(hutang.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();


                        table.Cell().Border(1).Padding(2).Text("Kas bersih yang di peroleh dari Aktivitas Operasi").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text(kasbersihoperasi.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();


                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);

                        table.Cell().Border(1).Padding(2).Text("Arus Kas dari Aktivitas Investasi").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);


                        table.Cell().Border(1).Padding(2).Text("Penambahan aset tetap").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text(pembelianperalatannegate.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();

                        table.Cell().Border(1).Padding(2).Text("Kas bersih yang digunakan untuk aktivitas investasi").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text(pembelianperalatannegate.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();

                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);


                        table.Cell().Border(1).Padding(2).Text("Arus Kas dari Aktivitas Pembiayaan").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);


                        table.Cell().Border(1).Padding(2).Text("Penambahan Aset Tetap").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text(datapembelianperalatan.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();


                        table.Cell().Border(1).Padding(2).Text("Modal").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text(modal.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();



                        table.Cell().Border(1).Padding(2).Text("Kas bersih yang digunakan untuk aktivitas investasi").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text(kasbersihinventasi.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();

                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);

                       
                        table.Cell().Border(1).Padding(2).Text("Penambahan Bersih Kas dan Bank").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text(penambahankas.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();

                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);

                        table.Cell().Border(1).Padding(2).Text("Kas dan Bank Awal Tahun").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);

                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);

                        table.Cell().Border(1).Padding(2).Text("Kas dan Bank Akhir Tahun").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text(penambahankas.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();



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
                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                    }
                    else

                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                obj.TB_D = datatb.TB_D;
                obj.TB_K = datatb.TB_K;
                obj.ispreview = true;
                byte[] pdfBytes = GeneratePdfV2(obj);
                var FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);


            }
            else
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var datajpb = db.JpbTbl.Where(y => y.TransDate.Month == month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => y.TransDate.Month == month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => y.TransDate.Month == month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
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
                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.totalint = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                    }
                    else

                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                obj.TB_D = datatb.TB_D;
                obj.TB_K = datatb.TB_K;
                obj.ispreview = true;
                byte[] pdfBytes = GeneratePdfV2(obj);
                var FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);

            }

        }

    }
}
