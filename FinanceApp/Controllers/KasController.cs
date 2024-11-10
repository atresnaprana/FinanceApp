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

namespace FinanceApp.Controllers
{
    public class KasController : Controller
    {
        private readonly FormDBContext db;
        public const string SessionKeyName = "TransDateStr";

        private IHostingEnvironment Environment;
        private readonly ILogger<KasController> _logger;

        public KasController(FormDBContext db, ILogger<KasController> logger, IHostingEnvironment _environment)
        {
            logger = logger;
            Environment = _environment;
            this.db = db;
        }
        public IActionResult Index()
        {
            var roles = ((ClaimsIdentity)User.Identity).Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            var data = new List<dbKas>();
            var transdatestr = HttpContext.Session.GetString(SessionKeyName);

            //if (!string.IsNullOrEmpty(edpCode) && !string.IsNullOrEmpty(SSCode))
            //{
            //    data = db.SSTable.Where(y => y.FLAG_AKTIF == 1 && y.EDP_CODE == edpCode && y.SS_CODE == SSCode).ToList();
            //    ViewData["EdpCode"] = edpCode;
            //    ViewData["SSCode"] = SSCode;

            //}
            //else
            //if (!string.IsNullOrEmpty(edpCode) && string.IsNullOrEmpty(SSCode))
            //{
            //    data = db.SSTable.Where(y => y.FLAG_AKTIF == 1 && y.EDP_CODE == edpCode).ToList();
            //    ViewData["EdpCode"] = edpCode;
            //    ViewData["SSCode"] = 0;
            //}
            //else
            //{
            //    ViewData["EdpCode"] = 0;
            //    ViewData["SSCode"] = 0;
            //}
            return View(data);
        }
    }
}
