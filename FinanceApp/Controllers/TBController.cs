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
using static QuestPDF.Helpers.Colors;

namespace FinanceApp.Controllers
{
    public class TBController : Controller
    {
        private readonly FormDBContext db;
        public const string SessionKeyName = "TransDateStr";

        private IHostingEnvironment Environment;
        private readonly ILogger<LRController> _logger;

        public TBController(FormDBContext db, ILogger<TBController> logger, IHostingEnvironment _environment)
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
                    var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000).ToList();
                    var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000).ToList();
                    var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000).ToList();

                    var datajpb = db.JpbTbl.Where(y => y.TransDate.Year == year).ToList();
                    var datajpn = db.JpnTbl.Where(y => y.TransDate.Year == year).ToList();
                    var datajm = db.JmTbl.Where(y => y.TransDate.Year == year).ToList();
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
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                        if(dt.account_no >= 1200006 && dt.account_no <= 1200009)
                        {
                            fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                            fld.Value_D_int =  (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

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
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);


                        fld.Value_K =  (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") ;
                        fld.Value_K_int =  (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                        TBData_K.Add(fld);



                    }
                    obj.TB_D = TBData_D;
                    obj.TB_K = TBData_K;
                    byte[] pdfBytes = GenerateTrialBalancePdf(obj);
                    var FileName = "Neraca" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                    return File(pdfBytes, "application/pdf", FileName);

                  
                }
                else
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000).ToList();
                    var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000).ToList();
                    var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000).ToList();
                    var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000).ToList();
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
                        fld.akundk = dt.akundk;

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
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);

                        if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                        {
                            fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.Value_D_int =  (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }
                        else
                        {
                            fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.Value_D_int =  (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

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
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);


                        fld.Value_K =  (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") ;
                        fld.Value_K_int =  (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                        TBData_K.Add(fld);



                    }
                    obj.TB_D = TBData_D;
                    obj.TB_K = TBData_K;

                    byte[] pdfBytes = GenerateTrialBalancePdf(obj);
                    var FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                    return File(pdfBytes, "application/pdf", FileName);

                   
                }

            }
            else
            {
                return RedirectToAction("GeneratePdf", new { message = "Incorrect Periode Settings" });
            }

        }

        public IActionResult GeneratePreview(string message)
        {
            ViewBag.Message = message;
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePreview([Bind] LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;

            // Render the "Index" view as a PDF

            if (isyearly)
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000).ToList();
                var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000).ToList();
                var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000).ToList();
                var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000).ToList();

                var datajpb = db.JpbTbl.Where(y => y.TransDate.Year == year).ToList();
                var datajpn = db.JpnTbl.Where(y => y.TransDate.Year == year).ToList();
                var datajm = db.JmTbl.Where(y => y.TransDate.Year == year).ToList();
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
                    var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
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
                    var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);


                    fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                    fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                    TBData_K.Add(fld);



                }
                obj.TB_D = TBData_D;
                obj.TB_K = TBData_K;
                byte[] pdfBytes = GenerateTrialBalancePdf(obj);
                var FileName = "Neraca" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);


            }
            else
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000).ToList();
                var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000).ToList();
                var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000).ToList();
                var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000).ToList();
                var datajpb = db.JpbTbl.Where(y => y.TransDate.Month == month && y.TransDate.Year == year).ToList();
                var datajpn = db.JpnTbl.Where(y => y.TransDate.Month == month && y.TransDate.Year == year).ToList();
                var datajm = db.JmTbl.Where(y => y.TransDate.Month == month && y.TransDate.Year == year).ToList();
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
                    var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);

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
                    var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);


                    fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                    fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                    TBData_K.Add(fld);



                }
                obj.TB_D = TBData_D;
                obj.TB_K = TBData_K;

                byte[] pdfBytes = GenerateTrialBalancePdf(obj);
                var FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);


            }

        }


        private byte[] GenerateTrialBalancePdf(LRModel obj)
        {
            using var stream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape()); // Set to Landscape
                    page.Margin(10); // Reduced margin
                    page.Header().Column(col =>
                    {
                        if(obj.ispreview == true)
                        {
                            col.Item().Text("Trial Balance Report (PREVIEW)")
                            .Bold()
                            .FontSize(12) // Reduced font size
                            .AlignCenter();
                        }
                        else
                        {
                          col.Item().Text("Trial Balance Report")
                            .Bold()
                            .FontSize(12) // Reduced font size
                            .AlignCenter();
                        }
                      

                        col.Item().Text("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
                            .FontSize(8)
                            .AlignCenter();

                        col.Item().Text("Company XYZ - Financial Report")
                            .FontSize(8)
                            .AlignCenter();
                    });

                    page.Content().Table(table =>
                    {
                        // Define Columns for Side-by-Side Format
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);  // ID column (left)
                            columns.RelativeColumn(3);  // Account Name (left)
                            columns.RelativeColumn(1.5f);  // Debit (left)

                            columns.RelativeColumn(1);  // ID column (right)
                            columns.RelativeColumn(3);  // Account Name (right)
                            columns.RelativeColumn(1.5f);  // Credit (right)

                        });

                        // Header Row
                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(2).Text("Account_No").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Account Name").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Debit").Bold().FontSize(8);

                            header.Cell().Border(1).Padding(2).Text("Account_No").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Account Name").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Credit").Bold().FontSize(8);

                        });
                        var dt_d = obj.TB_D.Where(y => y.AccountNo_D != 0).OrderBy(y => y.AccountNo_D).ToList();
                        var dt_k = obj.TB_K.Where(y => y.AccountNo_K != 0).OrderBy(y => y.AccountNo_K).ToList();
                        var length = 0;
                        if(dt_d.Count() > dt_k.Count())
                        {
                            length = dt_d.Count() + 7;
                        } else if (dt_d.Count() < dt_k.Count())
                        {
                            length = dt_k.Count() + 7;
                        }
                        string previousFirstDigit_D = null;

                        // Sample Data Rows - Side by Side
                        for (int i = 1; i <= length; i++)
                        {
                           
                            if (i < dt_d.Count())
                            {

                                var fld_D = dt_d[i];
                                string currentFirstDigit_D = fld_D.AccountNo_D.ToString().Substring(0, 2);
                                var max = dt_d.Where(y => y.AccountNo_D.ToString().Substring(0, 2) == currentFirstDigit_D).Max(y => y.AccountNo_D);
                                var last2digitstr = max.ToString().Substring(max.ToString().Length - 2);
                                var last2digit = Convert.ToInt32(last2digitstr);
                                var currentlast2digit = fld_D.AccountNo_D.ToString().Substring(fld_D.AccountNo_D.ToString().Length - 2);
                                //if (previousFirstDigit_D != currentFirstDigit_D && previousFirstDigit_D != null)
                                //{
                                //    var subtotal = obj.ReportModel.Where(y => y.akun.StartsWith(previousFirstDigit_D)).Sum(y => y.totalint);
                                //    table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                //    table.Cell().Border(1).Padding(2).Text("Jumlah Aktiva Lancar").FontSize(7);
                                //    table.Cell().Border(1).Padding(2).Text("").FontSize(7).AlignRight();
                                //    table.Cell().Border(1).Padding(2).Text(subtotal.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight(); // Debit 2 Value
                                //}
                                table.Cell().Border(1).Padding(2).Text(fld_D.AccountNo_D).FontSize(7);
                                table.Cell().Border(1).Padding(2).Text(fld_D.Desc_D).FontSize(7);
                                if (fld_D.akundk_D == "-")
                                {
                                    table.Cell().Border(1).Padding(2).Text("").FontSize(7).AlignRight();

                                }
                                else
                                {
                                    table.Cell().Border(1).Padding(2).Text(fld_D.Value_D).FontSize(7).AlignRight();

                                }

                                previousFirstDigit_D = currentFirstDigit_D;

                            }
                            else
                            {
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7).AlignRight(); // Debit Value

                            }
                            if (i < dt_k.Count())
                            {
                                var fld_K = dt_k[i];
                                table.Cell().Border(1).Padding(2).Text(fld_K.AccountNo_K).FontSize(7);
                                table.Cell().Border(1).Padding(2).Text(fld_K.Desc_K).FontSize(7);
                                if (fld_K.akundk_K == "-")
                                {
                                    table.Cell().Border(1).Padding(2).Text("").FontSize(7).AlignRight();

                                }
                                else
                                {
                                    if (fld_K.AccountNo_K == 3000002)
                                    {
                                        var subtotal3 = obj.ReportModel.Sum(y => y.totalint);
                                        table.Cell().Border(1).Padding(2).Text(subtotal3.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();

                                    }
                                    else
                                    {
                                        table.Cell().Border(1).Padding(2).Text(fld_K.Value_K).FontSize(7).AlignRight();

                                    }

                                }
                            }
                            else
                            {
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7).AlignRight(); // Debit Value

                            }


                            //table.Cell().Border(1).Padding(2).Text(i.ToString()).FontSize(7);
                            //table.Cell().Border(1).Padding(2).Text($"Account {i}").FontSize(7);
                            //table.Cell().Border(1).Padding(2).Text((i * 1000).ToString("C2")).FontSize(7).AlignRight(); // Debit Value

                            //table.Cell().Border(1).Padding(2).Text((i + 5).ToString()).FontSize(7);
                            //table.Cell().Border(1).Padding(2).Text($"Account {i + 5}").FontSize(7);
                            //table.Cell().Border(1).Padding(2).Text((i % 2 == 0 ? (i * 800).ToString("C2") : "-")).FontSize(7).AlignRight(); // Credit Value
                        }
                        var subtotalequity = obj.ReportModel.Sum(y => y.totalint);

                        var subtotal_D = dt_d.Sum(y => y.Value_D_int);
                        var subtotal_K = dt_k.Sum(y => y.Value_K_int) + subtotalequity;
                       

                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("Jumlah").Bold().FontSize(8);
                        table.Cell().Border(1).Padding(2).Text(subtotal_D.ToString("#,##0.00;(#,##0.00)")).Bold().FontSize(8).AlignRight(); // Debit Value
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("Jumlah").Bold().FontSize(8);
                        table.Cell().Border(1).Padding(2).Text(subtotal_K.ToString("#,##0.00;(#,##0.00)")).Bold().FontSize(8).AlignRight(); // Debit Value

                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Generated using QuestPDF").FontSize(7); // Smaller footer font
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }
    }
}
