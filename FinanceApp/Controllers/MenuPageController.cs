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


namespace BaseLineProject.Controllers
{
    public class MenuPageController : Controller
    {
        private readonly FormDBContext db;
        private IHostingEnvironment Environment;
        private readonly ILogger<MenuPageController> _logger;

        public MenuPageController(FormDBContext db, ILogger<MenuPageController> logger, IHostingEnvironment _environment)
        {
            logger = logger;
            Environment = _environment;
            this.db = db;
        }
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Index()
        {
            var data = new List<SystemMenuModel>();
            data = db.MenuTbl.Where(y => y.FLAG_AKTIF == "1").OrderBy(y => y.MENU_DESC).ToList();
            return View(data);
        }
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Create()
        {
            List<SystemTabModel> tablist = new List<SystemTabModel>();
            tablist = db.TabTbl.Where(y => y.FLAG_AKTIF == "1").ToList().Select(y => new SystemTabModel()
            {
                ID = y.ID,
                TAB_DESC = y.TAB_TXT + " - " + y.TAB_DESC
            }).ToList();
            SystemMenuModel fld = new SystemMenuModel();
            fld.ddTab = tablist.OrderBy(y => y.TAB_DESC).ToList();
            return View(fld);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind] SystemMenuModel obj)
        {
            if (ModelState.IsValid)
            {
                obj.ENTRY_DATE = DateTime.Now;
                obj.UPDATE_DATE = DateTime.Now;
                obj.ENTRY_USER = User.Identity.Name;
                obj.UPDATE_USER = User.Identity.Name;
                obj.FLAG_AKTIF = "1";
                try
                {

                    db.MenuTbl.Add(obj);
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
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            SystemMenuModel fld = db.MenuTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            else
            {
                List<SystemTabModel> tablist = new List<SystemTabModel>();
                tablist = db.TabTbl.Where(y => y.FLAG_AKTIF == "1").ToList().Select(y => new SystemTabModel()
                {
                    ID = y.ID,
                    TAB_DESC = y.TAB_TXT + " - " + y.TAB_DESC
                }).ToList();
                fld.ddTab = tablist;
            }
            return View(fld);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(int? id, [Bind] SystemMenuModel fld)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var editFld = db.MenuTbl.Find(id);
                editFld.MENU_DESC = fld.MENU_DESC;
                editFld.MENU_TXT = fld.MENU_TXT;
                editFld.MENU_LINK = fld.MENU_LINK;
                editFld.TAB_ID = fld.TAB_ID;

                editFld.UPDATE_DATE = DateTime.Now;
                editFld.UPDATE_USER = User.Identity.Name;
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
            SystemMenuModel fld = db.MenuTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            return View(fld);
        }
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMenu(int? id)
        {
            SystemMenuModel fld = db.MenuTbl.Find(id);
            fld.FLAG_AKTIF = "0";
            fld.UPDATE_DATE = DateTime.Now;
            fld.UPDATE_USER = User.Identity.Name;
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
