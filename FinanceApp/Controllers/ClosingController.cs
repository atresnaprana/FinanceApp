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

namespace FinanceApp.Controllers
{
    public class ClosingController : Controller
    {
        private readonly FormDBContext db;
        public const string SessionKeyName = "TransDateStrJm";

        private IHostingEnvironment Environment;
        private readonly ILogger<ClosingController> _logger;

        public ClosingController(FormDBContext db, ILogger<ClosingController> logger, IHostingEnvironment _environment)
        {
            logger = logger;
            Environment = _environment;
            this.db = db;
        }
        [Authorize(Roles = "AccountAdmin")]

        public IActionResult Index()
        {
            var data = new List<dbClosing>();
            data = db.ClosingTbl.OrderBy(y => y.id).ToList();
            return View(data);
        }
        [HttpGet]
        [Authorize(Roles = "AccountAdmin")]
        public IActionResult Create()
        {
            
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind] dbClosing obj)
        {
            if (ModelState.IsValid)
            {
                obj.entry_date = DateTime.Now;
                obj.update_date = DateTime.Now;
                obj.entry_user = User.Identity.Name;
                obj.update_user = User.Identity.Name;
                if (obj.isclosebool)
                {
                    obj.isclosed = "Y";
                }
                else
                {
                    obj.isclosed = "N";
                }
                try
                {

                    db.ClosingTbl.Add(obj);
                    db.SaveChanges();


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
                return RedirectToAction("Index");
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
            dbClosing fld = db.ClosingTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            else
            {
                if(fld.isclosed == "Y")
                {
                    fld.isclosebool = true;
                }
                else
                {
                    fld.isclosebool = false;
                }
            }
            return View(fld);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(int? id, [Bind] dbClosing fld)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var editFld = db.ClosingTbl.Find(id);
                editFld.description = fld.description;
                editFld.periode = fld.periode;
                editFld.year = fld.year;
                editFld.datefrom = fld.datefrom;
                editFld.dateto = fld.dateto;
                if (fld.isclosebool)
                {
                    editFld.isclosed = "Y";
                }
                else
                {
                    editFld.isclosed = "N";
                }
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
       
    }
}
