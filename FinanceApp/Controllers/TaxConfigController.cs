using BaseLineProject.Controllers;
using BaseLineProject.Data;
using BaseLineProject.Models;
using FinanceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Controllers
{
    public class TaxConfigController : Controller
    {
        private readonly FormDBContext db;
        private IHostingEnvironment Environment;
        private readonly ILogger<TabPageController> _logger;

        public TaxConfigController(FormDBContext db, ILogger<TaxConfigController> logger, IHostingEnvironment _environment)
        {
            logger = logger;
            Environment = _environment;
            this.db = db;
        }
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Index()
        {
            var data = new List<dbTaxConfig>();
            data = db.TaxConfigTbl.Where(y => y.flag_aktif == "1").OrderBy(y => y.id).ToList();
            return View(data);
        }
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind] dbTaxConfig obj)
        {
            obj.entry_date = DateTime.Now;
            obj.update_date = DateTime.Now;
            obj.entry_user = User.Identity.Name;
            obj.update_user = User.Identity.Name;
            obj.flag_aktif = "1";
            try
            {

                db.TaxConfigTbl.Add(obj);
                db.SaveChanges();
                return RedirectToAction("Index");


            }
            catch (Exception ex)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\ErrorLog");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath, "ErrMsgAdd" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".txt")))
                {
                    outputFile.WriteLine(ex.ToString());
                }
            }
            //apprDal.AddApproval(objApproval);
            return View(obj);
        }
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            dbTaxConfig fld = db.TaxConfigTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            return View(fld);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(int? id, [Bind] dbTaxConfig fld)
        {
            if (id == null)
            {
                return NotFound();
            }
            else
            {
                var editFld = db.TaxConfigTbl.Find(id);
                editFld.taxmode = fld.taxmode;
                editFld.taxtype = fld.taxtype;
                editFld.taxlimit = fld.taxlimit;
                editFld.taxlimitmin = fld.taxlimitmin;
                editFld.taxlimitmax = fld.taxlimitmax;
                editFld.taxpercentage = fld.taxpercentage;

                editFld.update_date = DateTime.Now;
                editFld.update_user = User.Identity.Name;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\ErrorLog");
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }
                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath, "ErrMsgEdit" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".txt")))
                    {
                        outputFile.WriteLine(ex.ToString());
                    }
                }
                return RedirectToAction("Index");
            }
           
            return View(fld);
        }
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            dbTaxConfig fld = db.TaxConfigTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            return View(fld);
        }
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteTab(int? id)
        {
            dbTaxConfig fld = db.TaxConfigTbl.Find(id);
            fld.flag_aktif = "0";
            fld.update_date = DateTime.Now;
            fld.update_user = User.Identity.Name;
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\ErrorLog");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath, "ErrMsgEdit" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".txt")))
                {
                    outputFile.WriteLine(ex.ToString());
                }
            }
            return RedirectToAction("Index");
        }
    }
}
