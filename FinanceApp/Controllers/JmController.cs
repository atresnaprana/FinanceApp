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
using MimeDetective.Storage.Xml.v2;

namespace FinanceApp.Controllers
{
    public class JmController : Controller
    {
        private readonly FormDBContext db;
        public const string SessionKeyNameFrom = "TransDateStrJmFrom";
        public const string SessionKeyNameTo = "TransDateStrJmTo";

        private IHostingEnvironment Environment;
        private readonly ILogger<KasController> _logger;

        public JmController(FormDBContext db, ILogger<JmController> logger, IHostingEnvironment _environment)
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
            var data = new List<dbJm>();
            var datefromstr = HttpContext.Session.GetString(SessionKeyNameFrom);
            var datetostr = HttpContext.Session.GetString(SessionKeyNameTo);
            var datefrom = Convert.ToDateTime(datefromstr);
            var dateto = Convert.ToDateTime(datetostr);
            if (!string.IsNullOrEmpty(datefromstr) && !string.IsNullOrEmpty(datetostr))
            {
                ViewData["datefromstr"] = datefromstr;
                ViewData["datetostr"] = datetostr;

                List<dbJm> TblDt = new List<dbJm>();
                TblDt = db.JmTbl.Where(y => y.flag_aktif == "1").ToList();
                var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

                var tbldt2 = TblDt.Select(y => new dbJm()
                {
                    TransDate = y.TransDate,
                    TransDateStr = y.TransDate.ToString("yyyy-MM-dd"),
                    Description = y.Description,
                    Trans_no = y.Trans_no,
                    Akun_Debit = y.Akun_Debit,
                    Akun_Credit = y.Akun_Credit,
                    Debit = y.Debit,
                    Credit = y.Credit,
                    DebitStr = y.Debit.ToString("#,##0.00"),
                    CreditStr = y.Credit.ToString("#,##0.00"),
                    id = y.id,
                    company_id = y.company_id
                }).Where(y => y.TransDate >= datefrom && y.TransDate <= dateto && y.company_id == datas.COMPANY_ID).ToList();
                data = tbldt2;
            }
            else
            {
                ViewData["datefromstr"] = null;
                ViewData["datetostr"] = null;
            }

            return View(data);
        }

        public JsonResult getTbl(string from, string to)
        {
            HttpContext.Session.SetString(SessionKeyNameFrom, from);
            HttpContext.Session.SetString(SessionKeyNameTo, to);
            var datefrom = Convert.ToDateTime(from);
            var dateto = Convert.ToDateTime(to);
            var data = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            //Creating List    
            List<dbJm> TblDt = new List<dbJm>();
            TblDt = db.JmTbl.Where(y => y.flag_aktif == "1").ToList();
            var tbldt2 = TblDt.Select(y => new dbJm()
            {
                TransDate = y.TransDate,
                TransDateStr = y.TransDate.ToString("yyyy-MM-dd"),
                Description = y.Description,
                Trans_no = y.Trans_no,
                Akun_Debit = y.Akun_Debit,
                Akun_Credit = y.Akun_Credit,
                Debit = y.Debit,
                Credit = y.Credit,
                DebitStr = y.Debit.ToString("#,##0.00"),
                CreditStr = y.Credit.ToString("#,##0.00"),
                id = y.id,
                company_id = y.company_id
            }).Where(y => y.TransDate >= datefrom && y.TransDate <= dateto && y.company_id == data.COMPANY_ID).ToList();
            return Json(tbldt2);
        }
        public JsonResult getTblEmpty()
        {
            HttpContext.Session.SetString(SessionKeyNameFrom, "");
            HttpContext.Session.SetString(SessionKeyNameTo, "");

            //Creating List    
            List<dbJm> TblDt = new List<dbJm>();

            return Json(TblDt);
        }
        [HttpGet]
        //[Authorize(Roles = "SuperAdmin")]
        public IActionResult Create()
        {
            var data = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            List<dbAccount> acclist = new List<dbAccount>();
            acclist = db.AccountTbl.Where(y => y.flag_aktif == "1" && y.company_id == data.COMPANY_ID).ToList().Select(y => new dbAccount()
            {
                account_no = y.account_no,
                account_name = y.account_no.ToString() + " - " + y.account_name
            }).ToList();
            dbJm fld = new dbJm();
              
            fld.TransDate = DateTime.Now;
            fld.dddbacc = acclist.OrderBy(y => y.account_no).ToList();
            fld.company_id = data.COMPANY_ID;
            var existingsales = db.JmTbl.Where(y => y.company_id == data.COMPANY_ID).ToList();
            var number = "0001";
            var trans_nodata = existingsales.Select(y => new dbJm()
            {
                Trans_no = y.Trans_no,
                shorttransno = y.Trans_no.Substring(0, 9),
                lasttransno = Convert.ToInt32(y.Trans_no.Substring(9, 4))


            }).ToList();

            var checkinvoicecurrent = trans_nodata.Where(y => y.shorttransno.Split("JM_")[1] == DateTime.Now.ToString("ddMMyy")).ToList();

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
            fld.Trans_no = "JM_" + DateTime.Now.ToString("ddMMyy") + number;

            return View(fld);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind] dbJm obj)
        {
            if (ModelState.IsValid)
            {
                var data = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

                var dataclosing = db.ClosingTbl.Where(y => y.datefrom <= obj.TransDate && y.dateto >= obj.TransDate && y.isclosed == "Y" && y.company_id == data.COMPANY_ID).ToList();
                if (dataclosing.Count == 0)
                {
                    var transdate = obj.TransDate;


                    var existingsales = db.JmTbl.Where(y => y.company_id == data.COMPANY_ID).ToList();
                    var number = "0001";
                    var trans_nodata = existingsales.Select(y => new dbJm()
                    {
                        Trans_no = y.Trans_no,
                        shorttransno = y.Trans_no.Substring(0, 9),
                        lasttransno = Convert.ToInt32(y.Trans_no.Substring(9, 4))


                    }).ToList();

                    var checkinvoicecurrent = trans_nodata.Where(y => y.shorttransno.Split("JM_")[1] == transdate.ToString("ddMMyy")).ToList();

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
                    obj.Trans_no = "JM_" + transdate.ToString("ddMMyy") + number;

                    obj.entry_date = DateTime.Now;
                    obj.update_date = DateTime.Now;
                    obj.entry_user = User.Identity.Name;
                    obj.update_user = User.Identity.Name;
                    obj.flag_aktif = "1";
                    var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
                    obj.company_id = datas.COMPANY_ID;
                    try
                    {
                        db.JmTbl.Add(obj);
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
                else
                {
                    obj.errormessage = "Periode sudah di closing";
                    List<dbAccount> acclist = new List<dbAccount>();
                    acclist = db.AccountTbl.Where(y => y.flag_aktif == "1").ToList().Select(y => new dbAccount()
                    {
                        account_no = y.account_no,
                        account_name = y.account_no.ToString() + " - " + y.account_name
                    }).ToList();
                    obj.dddbacc = acclist.OrderBy(y => y.account_no).ToList();
                    return View(obj);

                }

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
            dbJm fld = db.JmTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            else
            {
                var data = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

                List<dbAccount> acclist = new List<dbAccount>();
                acclist = db.AccountTbl.Where(y => y.flag_aktif == "1" && y.company_id == data.COMPANY_ID).ToList().Select(y => new dbAccount()
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
        public IActionResult Edit(int? id, [Bind] dbJm fld)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var editFld = db.JmTbl.Find(id);
                editFld.TransDate = fld.TransDate;
                editFld.Trans_no = fld.Trans_no;
                editFld.Description = fld.Description;
                editFld.Akun_Debit = fld.Akun_Debit;
                editFld.Akun_Credit = fld.Akun_Credit;
                editFld.Credit = fld.Credit;
                editFld.Debit = fld.Debit;
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
            dbJm fld = db.JmTbl.Find(id);
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
            dbJm fld = db.JmTbl.Find(id);
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
