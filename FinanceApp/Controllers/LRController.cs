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
using Rotativa.AspNetCore;

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
        public IActionResult GeneratePdf([Bind] LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.periode == month && y.isclosed == "Y").ToList();

            // Render the "Index" view as a PDF
           
            if(dataclosing.Count > 0)
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
                    return new ViewAsPdf("LR_Rpt", obj)
                    {
                        FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf",
                        PageSize = Rotativa.AspNetCore.Options.Size.A4,
                        PageMargins = new Rotativa.AspNetCore.Options.Margins
                        {
                            Left = 5,   // Set narrow margin for the left (in millimeters)
                            Right = 5,  // Set narrow margin for the right (in millimeters)
                            Top = 10,   // Set narrow margin for the top (in millimeters)
                            Bottom = 10 // Set narrow margin for the bottom (in millimeters)
                        }
                    };
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

                    return new ViewAsPdf("LR_Rpt", obj)
                    {
                        FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf",
                        PageSize = Rotativa.AspNetCore.Options.Size.A4,
                        PageMargins = new Rotativa.AspNetCore.Options.Margins
                        {
                            Left = 5,   // Set narrow margin for the left (in millimeters)
                            Right = 5,  // Set narrow margin for the right (in millimeters)
                            Top = 10,   // Set narrow margin for the top (in millimeters)
                            Bottom = 10 // Set narrow margin for the bottom (in millimeters)
                        }
                    };
                }
               
            }
            else
            {
                return RedirectToAction("GeneratePdf", new { message = "Incorrect Periode Settings" });
            }

        }
    }
}
