﻿using System;
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
            if (!string.IsNullOrEmpty(transdatestr))
            {
                ViewData["TransDateStr"] = transdatestr;
                List<dbKas> TblDt = new List<dbKas>();
                TblDt = db.KastTbl.Where(y => y.flag_aktif == "1").ToList();
                var tbldt2 = TblDt.Select(y => new dbKas()
                {
                    TransDate = y.TransDate,
                    TransDateStr = y.TransDate.ToString("yyyy-MM-dd"),
                    Description = y.Description,
                    Trans_no = y.Trans_no,
                    Akun_Debit = y.Akun_Debit,
                    Akun_Credit = y.Akun_Credit,
                    Debit = y.Debit,
                    Credit = y.Credit,
                    Saldo = y.Saldo,
                    DebitStr = y.Debit.ToString("#,##0.00"),
                    CreditStr = y.Credit.ToString("#,##0.00"),
                    SaldoStr = y.Saldo.ToString("#,##0.00"),
                    id = y.id
                }).Where(y => y.TransDateStr == transdatestr).ToList();
                data = tbldt2;
            }
            else
            {
                ViewData["TransDateStr"] = null;
            }
           
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
                DebitStr = y.Debit.ToString("#,##0.00"),
                CreditStr = y.Credit.ToString("#,##0.00"),
                SaldoStr = y.Saldo.ToString("#,##0.00"),
                id = y.id
            }).Where(y => y.TransDateStr == transdate).ToList();
            return Json(tbldt2);
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
                Trans_no = y.Trans_no,
                shorttransno = y.Trans_no.Substring(0, 8),
                lasttransno = Convert.ToInt32(y.Trans_no.Substring(8, 4))


            }).ToList();
            
            var checkinvoicecurrent = trans_nodata.Where(y => y.shorttransno.Split("K_")[1] == DateTime.Now.ToString("ddMMyy")).ToList();

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
            dbKas fld = db.KastTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            else
            {
                List<dbAccount> acclist = new List<dbAccount>();
                acclist = db.AccountTbl.Where(y => y.flag_aktif == "1").ToList().Select(y => new dbAccount()
                {
                    account_no = y.account_no,
                    account_name = y.account_no.ToString() + " - " + y.account_name
                }).ToList();
                fld.dddbacc = acclist;
               

            }
            return View(fld);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(int? id, [Bind] dbKas fld)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var editFld = db.KastTbl.Find(id);
                editFld.TransDate = fld.TransDate;
                editFld.Trans_no = fld.Trans_no;
                editFld.Description = fld.Description;
                editFld.Akun_Debit = fld.Akun_Debit;
                editFld.Akun_Credit = fld.Akun_Credit;
                editFld.Credit = fld.Credit;
                editFld.Debit = fld.Debit;
                editFld.Saldo = fld.Saldo;
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
        //[Authorize(Roles = "SuperAdmin")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            dbKas fld = db.KastTbl.Find(id);
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
            dbKas fld = db.KastTbl.Find(id);
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
