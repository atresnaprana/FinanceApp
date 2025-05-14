using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BaseLineProject.Data;
using BaseLineProject.Models;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Authorization;
using BaseLineProject.Controllers;
using FinanceApp.Models;

namespace FinanceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly FormDBContext db;
        private IHostingEnvironment Environment;
        private readonly ILogger<AccountController> _logger;
        public AccountController(FormDBContext db, ILogger<AccountController> logger, IHostingEnvironment _environment)
        {
            logger = logger;
            Environment = _environment;
            this.db = db;
        }
        [Authorize(Roles = "AccountAdmin")]
        public IActionResult Index()
        {
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            var data = new List<dbAccount>();
            data = db.AccountTbl.Where(y => y.flag_aktif == "1" && y.company_id == datas.COMPANY_ID).OrderBy(y => y.account_no).ToList();
            return View(data);
        }
        [HttpGet]
        [Authorize(Roles = "AccountAdmin")]
        public IActionResult Create()
        {
            dbAccount fld = new dbAccount();
            return View(fld);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind] dbAccount obj)
        {
            if (ModelState.IsValid)
            {
                obj.entry_date = DateTime.Now;
                obj.update_date = DateTime.Now;
                obj.entry_user = User.Identity.Name;
                obj.update_user = User.Identity.Name;
                obj.flag_aktif = "1";
                var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
                obj.company_id = datas.COMPANY_ID;
                try
                {
                    dbAccount edpDt = db.AccountTbl.Where(y => y.account_no == obj.account_no).FirstOrDefault();
                    if (edpDt != null)
                    {
                        obj.errormessage = "nomor akun sudah terdaftar";

                    }
                    else
                    {
                        db.AccountTbl.Add(obj);
                        db.SaveChanges();
                        return RedirectToAction("Index");

                    }



                }
                catch (Exception ex)
                {
                    obj.errormessage = ex.InnerException.Message;
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
            }
            return View(obj);
        }
        [Authorize(Roles = "AccountAdmin")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            dbAccount fld = db.AccountTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            return View(fld);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(int? id, [Bind] dbAccount fld)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var editFld = db.AccountTbl.Find(id);
                editFld.hierarchy = fld.hierarchy;
                editFld.account_name  = fld.account_name;
                editFld.akundk = fld.akundk;
                editFld.akunnrlr = fld.akunnrlr;
                editFld.update_date = DateTime.Now;
                editFld.update_user = User.Identity.Name;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    editFld.errormessage = ex.InnerException.Message;
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
        
        [Authorize(Roles = "AccountAdmin")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            dbAccount fld = db.AccountTbl.Find(id);
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
            dbAccount fld = db.AccountTbl.Find(id);
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
