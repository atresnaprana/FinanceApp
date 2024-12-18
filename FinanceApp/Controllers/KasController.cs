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

        public JsonResult getTbl(string transdate)
        {
            HttpContext.Session.SetString(SessionKeyName, transdate);

            //Creating List    
            List<dbKas> TblDt = new List<dbKas>();
            TblDt = db.KastTbl.Where(y => y.flag_aktif == "1").ToList();
            var tbldt2 = TblDt.Select(y => new dbKas()
            {
                TransDateStr = y.TransDate.ToString("yyyy-MM-dd"),
                Description = y.Description,
                Trans_no = y.Trans_no,
                Akun_Debit = y.Akun_Debit,
                Akun_Credit = y.Akun_Credit,
                Debit = y.Debit,
                Credit = y.Credit,
                Saldo = y.Saldo,
                id = y.id
            }).Where(y => y.TransDateStr == transdate).ToList();
            return Json(TblDt);
        }
        public JsonResult getTblEmpty()
        {
            HttpContext.Session.SetString(SessionKeyName, "");

            //Creating List    
            List<dbKas> TblDt = new List<dbKas>();

            return Json(TblDt);
        }
        [HttpGet]
        //[Authorize(Roles = "SuperAdmin")]
        public IActionResult Create()
        {
            List<dbAccount> acclist = new List<dbAccount>();
            acclist = db.AccountTbl.Where(y => y.flag_aktif == "1").ToList().Select(y => new dbAccount()
            {
                account_no = y.account_no,
                account_name = y.account_no.ToString() + " - " + y.account_name
            }).ToList();
            dbKas fld = new dbKas();
            fld.TransDate = DateTime.Now;
            fld.dddbacc = acclist.OrderBy(y => y.account_no).ToList();
            var existingsales = db.KastTbl.ToList();
            var number = "0001";
            var trans_nodata = existingsales.Select(y => new dbKas()
            {
                shorttransno = y.Trans_no.Substring(0, 8),
                lasttransno = Convert.ToInt32(y.Trans_no.Substring(6, 4))


            }).ToList();
            
            var checkinvoicecurrent = trans_nodata.Where(y => y.shorttransno == DateTime.Now.ToString("ddMMyy")).ToList();

            if (checkinvoicecurrent.Count > 0)
            {
                var chkinvnum = checkinvoicecurrent.Max(y => y.lasttransno) + 1;
                if (chkinvnum.ToString().Length == 1)
                {
                    number = "000" + chkinvnum.ToString();
                }
                else if (chkinvnum.ToString().Length == 2)
                {
                    number = "00" + chkinvnum.ToString();
                }
                else if (chkinvnum.ToString().Length == 3)
                {
                    number = "0" + chkinvnum.ToString();
                }
                else if (chkinvnum.ToString().Length == 4)
                {
                    number = chkinvnum.ToString();
                }
            }
            fld.Trans_no = "K_" + DateTime.Now.ToString("ddMMyy") + number;

            return View(fld);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind] dbKas obj)
        {
            if (ModelState.IsValid)
            {
                obj.entry_date  = DateTime.Now;
                obj.update_date = DateTime.Now;
                obj.entry_user = User.Identity.Name;
                obj.update_user = User.Identity.Name;
                obj.flag_aktif = "1";
                try
                {

                    db.KastTbl.Add(obj);
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
        //[Authorize(Roles = "SuperAdmin")]
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
        //[Authorize(Roles = "SuperAdmin")]
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
