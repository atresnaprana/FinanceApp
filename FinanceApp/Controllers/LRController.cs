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
        public IActionResult LR_Rpt(LRModel datas)
        {
            // Pass any necessary data to the view via the ViewData or ViewBag
            
            ViewData["ReportTitle"] = "Sample Report";
            List<LRRptModel> rptdata = new List<LRRptModel>();
            var dataakun = datas.akundata;
            foreach(var dt in dataakun)
            {

            }

            return View();
        }

        public IActionResult GeneratePdf([Bind] LRModel obj)
        {
            // Render the "Index" view as a PDF
            var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000).ToList();
            var datajpb = db.JpbTbl.ToList();
            var datajpn = db.JpnTbl.ToList();
            var datajm = db.JmTbl.ToList();
            var dataclosing = db.ClosingTbl.ToList();
            obj.akundata = dataacc;
            obj.jpbdata = datajpb;
            obj.jpndata = datajpn;
            obj.jmdata = datajm;
            obj.closingdata = dataclosing;
            
            return new ViewAsPdf("LR_Rpt", obj)
            {
                FileName = "Sample.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4
            };
        }
    }
}
