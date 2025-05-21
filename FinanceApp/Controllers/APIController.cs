using BaseLineProject.Data;
using FinanceApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MimeDetective.Storage.Xml.v2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using QuestPDF.Infrastructure;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using BaseLineProject.Models;

namespace FinanceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class APIController : Controller
    {
        private readonly FormDBContext db;
        public const string SessionKeyNameFrom = "TransDateStrJmFrom";
        public const string SessionKeyNameTo = "TransDateStrJmTo";

        private IHostingEnvironment Environment;
        private readonly ILogger<KasController> _logger;

        public APIController(FormDBContext db, ILogger<JmController> logger, IHostingEnvironment _environment)
        {
            logger = logger;
            Environment = _environment;
            this.db = db;
        }
        #region datamanagement
        [Authorize(Roles = "getdatauser")]
        public IActionResult getdatauser()
        {
            var dt = new UserModel();
            var data = new List<dbCustomer>();
            var currentcompany = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            var companyid = currentcompany.COMPANY_ID;
            data = db.CustomerTbl.Where(y => y.COMPANY_ID == companyid).OrderBy(y => y.id).ToList();

            var counttotaluser = db.CustomerTbl.Where(y => y.COMPANY_ID == companyid).Count();
            var isallowed = true;
            dt.pkg = currentcompany.VA1NOTE;
            if (currentcompany.VA1NOTE == "Basic")
            {
                return NotFound();
            }
            else if (currentcompany.VA1NOTE == "UMKM")
            {
                if (counttotaluser >= 3)
                {
                    isallowed = false;
                }
            }
            else if (currentcompany.VA1NOTE == "Enterprise")
            {
                var val = Convert.ToInt32(currentcompany.VA1);
                if (counttotaluser >= val)
                {
                    isallowed = false;
                }
            }
            ViewData["isallowed"] = isallowed;
            dt.customerdt = data;
            return Json(dt);
        }



        [Authorize]
        [HttpGet("getdataAccount")]
        public IActionResult getdataAccount()
        {
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Invalid token or missing username" });
            var data = new List<dbAccount>();
            data = db.AccountTbl.Where(y => y.flag_aktif == "1" && y.company_id == datas.COMPANY_ID).OrderBy(y => y.account_no).ToList();
            return Json(data);
        }
        [Authorize]
        [HttpPost("CreateAccount")]
        public IActionResult CreateAccount([FromBody] dbAccount obj)
        {
            if (ModelState.IsValid)
            {
                var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
                obj.entry_date = DateTime.Now;
                obj.update_date = DateTime.Now;
                obj.entry_user = User.Identity.Name;
                obj.update_user = User.Identity.Name;
                obj.flag_aktif = "1";
                obj.errormessage = "ok";
                obj.company_id = datas.COMPANY_ID;
                try
                {
                    dbAccount edpDt = db.AccountTbl.Where(y => y.account_no == obj.account_no && y.company_id == datas.COMPANY_ID).FirstOrDefault();
                    if (edpDt != null)
                    {
                        obj.errormessage = "nomor akun sudah terdaftar";
                    }
                    else
                    {
                        db.AccountTbl.Add(obj);
                        db.SaveChanges();
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
            return Ok(new { message = obj.errormessage });
        }
        
        [Authorize]
        [HttpGet("geteditfield")]
        public IActionResult geteditfield(int? id)
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
            return Json(fld);
        }
        [Authorize]
        [HttpPost("EditAccount")]
        public IActionResult EditAccount(int? id, [FromBody] dbAccount fld)
        {
            if (id == null)
            {
                return NotFound();
            }
            fld.errormessage = "ok";
            if (ModelState.IsValid)
            {
                var editFld = db.AccountTbl.Find(id);
                editFld.hierarchy = fld.hierarchy;
                editFld.account_name = fld.account_name;
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
                    fld.errormessage = ex.InnerException.Message;
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
            }
            return Ok(new { message = fld.errormessage });
        }

        [Authorize]
        [HttpPost("DeleteFld")]
        public IActionResult DeleteFld(int? id)
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
            return Json(fld);
        }
        [Authorize]
        [HttpPost("DeleteAccount")]
        public IActionResult DeleteAccount(int? id)
        {
            dbAccount fld = db.AccountTbl.Find(id);
            fld.flag_aktif = "0";
            fld.update_date = DateTime.Now;
            fld.update_user = User.Identity.Name;
            fld.errormessage = "ok";
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                fld.errormessage = ex.Message.ToString();
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
            return Ok(new { message = fld.errormessage });
        }
        [Authorize]
        [HttpGet("getdataJM")]
        public IActionResult getdataJM(DateTime? fromDate, DateTime? toDate)
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Invalid token or missing username" });
            List<dbJm> TblDt = new List<dbJm>();
            TblDt = db.JmTbl.Where(y => y.flag_aktif == "1").ToList();
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            var tbldt2 = new List<dbJm>();
            if (fromDate == null || toDate == null)
            {

                tbldt2 = TblDt.Select(y => new dbJm()
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
                }).Where(y =>  y.company_id == datas.COMPANY_ID).ToList();
            }
            else
            {

                tbldt2 = TblDt.Select(y => new dbJm()
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
                }).Where(y => y.TransDate >= fromDate && y.TransDate <= toDate && y.company_id == datas.COMPANY_ID).ToList();
            }
            
            return Json(tbldt2);

        }
        [Authorize]
        [HttpGet("getddAccount")]
        public IActionResult getddAccount(DateTime? fromDate, DateTime? toDate)
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Invalid token or missing username" });
            List<dbJm> TblDt = new List<dbJm>();
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            List<dbAccount> acclist = new List<dbAccount>();
            acclist = db.AccountTbl.Where(y => y.flag_aktif == "1" && y.company_id == datas.COMPANY_ID).ToList().Select(y => new dbAccount()
            {
                account_no = y.account_no,
                account_name = y.account_no.ToString() + " - " + y.account_name
            }).ToList();

            return Json(acclist);

        }
        [Authorize]
        [HttpGet("getlastnoJM")]
        public IActionResult getlastnoJM()
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Invalid token or missing username" });
            List<dbJm> TblDt = new List<dbJm>();
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            var existingsales = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList();
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
            string transno = "JM_" + DateTime.Now.ToString("ddMMyy") + number;
            return Json(transno);
        }

        [Authorize]
        [HttpPost("submitJM")]
        public IActionResult SubmitJM([FromBody] dbJm obj)
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Invalid token or missing username" });
            // Get current user's email
            var userEmail = User.Identity.Name;
            var datacompany = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            var dataclosing = db.ClosingTbl.Where(y => y.datefrom <= obj.TransDate && y.dateto >= obj.TransDate && y.isclosed == "Y" && y.company_id == datacompany.COMPANY_ID).ToList();
            if (dataclosing.Count() == 0)
            {
                var transdate = obj.TransDate;

                var errormsg = "Transaksi sudah di simpan";
                var existingsales = db.JmTbl.Where(y => y.company_id == datacompany.COMPANY_ID).ToList();
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
                    errormsg = ex.ToString();
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
                return Ok(new { message = errormsg });
            }
            else
            {
               
                return Ok(new { message = "Periode Sudah di closing" });

            }


        }


        [Authorize]
        [HttpGet("getdataJPB")]
        public IActionResult getdataJPB(DateTime? fromDate, DateTime? toDate)
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Invalid token or missing username" });
            List<dbJpb> TblDt = new List<dbJpb>();
            TblDt = db.JpbTbl.Where(y => y.flag_aktif == "1").ToList();
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            var tbldt2 = new List<dbJpb>();
            if (fromDate == null || toDate == null)
            {

                tbldt2 = TblDt.Select(y => new dbJpb()
                {
                    TransDate = y.TransDate,
                    TransDateStr = y.TransDate.ToString("yyyy-MM-dd"),
                    Description = y.Description,
                    Trans_no = y.Trans_no,
                    Akun_Debit = y.Akun_Debit,
                    Akun_Credit = y.Akun_Credit,
                    Akun_Debit_disc = y.Akun_Debit_disc,
                    Akun_Credit_disc = y.Akun_Credit_disc,
                    Value = y.Value,
                    Value_Disc = y.Value_Disc,
                    ValueStr = y.Value.ToString("#,##0.00"),
                    ValueDiscStr = y.Value_Disc.ToString("#,##0.00"),
                    company_id = y.company_id,
                    id = y.id
                }).Where(y => y.company_id == datas.COMPANY_ID).ToList();
            }
            else
            {

                tbldt2 = TblDt.Select(y => new dbJpb()
                {
                    TransDate = y.TransDate,
                    TransDateStr = y.TransDate.ToString("yyyy-MM-dd"),
                    Description = y.Description,
                    Trans_no = y.Trans_no,
                    Akun_Debit = y.Akun_Debit,
                    Akun_Credit = y.Akun_Credit,
                    Akun_Debit_disc = y.Akun_Debit_disc,
                    Akun_Credit_disc = y.Akun_Credit_disc,
                    Value = y.Value,
                    Value_Disc = y.Value_Disc,
                    ValueStr = y.Value.ToString("#,##0.00"),
                    ValueDiscStr = y.Value_Disc.ToString("#,##0.00"),
                    company_id = y.company_id,
                    id = y.id
                }).Where(y => y.TransDate >= fromDate && y.TransDate <= toDate && y.company_id == datas.COMPANY_ID).ToList();
            }

            return Json(tbldt2);

        }

        [Authorize]
        [HttpGet("getlastnoJPB")]
        public IActionResult getlastnoJPB()
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Invalid token or missing username" });
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            var existingsales = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList();
            var number = "0001";
            var trans_nodata = existingsales.Select(y => new dbJpb()
            {
                Trans_no = y.Trans_no,
                shorttransno = y.Trans_no.Substring(0, 10),
                lasttransno = Convert.ToInt32(y.Trans_no.Substring(10, 4))


            }).ToList();

            var checkinvoicecurrent = trans_nodata.Where(y => y.shorttransno.Split("JPB_")[1] == DateTime.Now.ToString("ddMMyy")).ToList();

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
            string transno = "JPB_" + DateTime.Now.ToString("ddMMyy") + number;

            return Json(transno);
        }

        [Authorize]
        [HttpPost("SubmitJPB")]
        public IActionResult SubmitJPB([FromBody] dbJpb obj)
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Invalid token or missing username" });
            // Get current user's email
            var userEmail = User.Identity.Name;
            var datacompany = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            var dataclosing = db.ClosingTbl.Where(y => y.datefrom <= obj.TransDate && y.dateto >= obj.TransDate && y.isclosed == "Y" && y.company_id == datacompany.COMPANY_ID).ToList();
            if (dataclosing.Count() == 0)
            {
                var errormsg = "Transaksi sudah di simpan";

                var transdate = obj.TransDate;

                var existingsales = db.JpbTbl.Where(y => y.company_id == datacompany.COMPANY_ID).ToList();
                var number = "0001";
                var trans_nodata = existingsales.Select(y => new dbJpb()
                {
                    Trans_no = y.Trans_no,
                    shorttransno = y.Trans_no.Substring(0, 10),
                    lasttransno = Convert.ToInt32(y.Trans_no.Substring(10, 4))


                }).ToList();

                var checkinvoicecurrent = trans_nodata.Where(y => y.shorttransno.Split("JPB_")[1] == transdate.ToString("ddMMyy")).ToList();

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
                obj.Trans_no = "JPB_" + transdate.ToString("ddMMyy") + number;

                obj.entry_date = DateTime.Now;
                obj.update_date = DateTime.Now;
                obj.entry_user = User.Identity.Name;
                obj.update_user = User.Identity.Name;
                obj.flag_aktif = "1";
                var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
                obj.company_id = datas.COMPANY_ID;
                try
                {

                    db.JpbTbl.Add(obj);
                    db.SaveChanges();


                }
                catch (Exception ex)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\ErrorLog");
                    errormsg = ex.ToString();
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
                return Ok(new { message = errormsg });
            }
            else
            {
              
                return Ok(new { message = "Periode Sudah di closing" });
            }


        }

        [Authorize]
        [HttpGet("getdataJPN")]
        public IActionResult getdataJPN(DateTime? fromDate, DateTime? toDate)
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Invalid token or missing username" });
            List<dbJpn> TblDt = new List<dbJpn>();
            TblDt = db.JpnTbl.Where(y => y.flag_aktif == "1").ToList();
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            var tbldt2 = new List<dbJpn>();
            if (fromDate == null || toDate == null)
            {

                tbldt2 = TblDt.Select(y => new dbJpn()
                {
                    TransDate = y.TransDate,
                    TransDateStr = y.TransDate.ToString("yyyy-MM-dd"),
                    Description = y.Description,
                    Trans_no = y.Trans_no,
                    Akun_Debit = y.Akun_Debit,
                    Akun_Credit = y.Akun_Credit,
                    Akun_Debit_disc = y.Akun_Debit_disc,
                    Akun_Credit_disc = y.Akun_Credit_disc,
                    Value = y.Value,
                    Value_Disc = y.Value_Disc,
                    ValueStr = y.Value.ToString("#,##0.00"),
                    ValueDiscStr = y.Value_Disc.ToString("#,##0.00"),
                    company_id = y.company_id,
                    id = y.id
                }).Where(y => y.company_id == datas.COMPANY_ID).ToList();
            }
            else
            {

                tbldt2 = TblDt.Select(y => new dbJpn()
                {
                    TransDate = y.TransDate,
                    TransDateStr = y.TransDate.ToString("yyyy-MM-dd"),
                    Description = y.Description,
                    Trans_no = y.Trans_no,
                    Akun_Debit = y.Akun_Debit,
                    Akun_Credit = y.Akun_Credit,
                    Akun_Debit_disc = y.Akun_Debit_disc,
                    Akun_Credit_disc = y.Akun_Credit_disc,
                    Value = y.Value,
                    Value_Disc = y.Value_Disc,
                    ValueStr = y.Value.ToString("#,##0.00"),
                    ValueDiscStr = y.Value_Disc.ToString("#,##0.00"),
                    company_id = y.company_id,
                    id = y.id
                }).Where(y => y.TransDate >= fromDate && y.TransDate <= toDate && y.company_id == datas.COMPANY_ID).ToList();
            }

            return Json(tbldt2);

        }

        [Authorize]
        [HttpGet("getlastnoJPN")]
        public IActionResult getlastnoJPN()
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Invalid token or missing username" });
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            var existingsales = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList();
            var number = "0001";
            var trans_nodata = existingsales.Select(y => new dbJpn()
            {
                Trans_no = y.Trans_no,
                shorttransno = y.Trans_no.Substring(0, 10),
                lasttransno = Convert.ToInt32(y.Trans_no.Substring(10, 4))


            }).ToList();

            var checkinvoicecurrent = trans_nodata.Where(y => y.shorttransno.Split("JPN_")[1] == DateTime.Now.ToString("ddMMyy")).ToList();

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
            string transno = "JPN_" + DateTime.Now.ToString("ddMMyy") + number;
            return Json(transno);
        }

        [Authorize]
        [HttpPost("SubmitJPN")]
        public IActionResult SubmitJPN([FromBody] dbJpn obj)
        {
            var username = User.Identity.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized(new { message = "Invalid token or missing username" });
            // Get current user's email
            var userEmail = User.Identity.Name;
            var datacompany = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            var dataclosing = db.ClosingTbl.Where(y => y.datefrom <= obj.TransDate && y.dateto >= obj.TransDate && y.isclosed == "Y" && y.company_id == datacompany.COMPANY_ID).ToList();
            if (dataclosing.Count() == 0)
            {
                var errormsg = "Transaksi sudah di simpan";

                var transdate = obj.TransDate;

                var existingsales = db.JpnTbl.Where(y => y.company_id == datacompany.COMPANY_ID).ToList();
                var number = "0001";
                var trans_nodata = existingsales.Select(y => new dbJpn()
                {
                    Trans_no = y.Trans_no,
                    shorttransno = y.Trans_no.Substring(0, 10),
                    lasttransno = Convert.ToInt32(y.Trans_no.Substring(10, 4))


                }).ToList();

                var checkinvoicecurrent = trans_nodata.Where(y => y.shorttransno.Split("JPN_")[1] == transdate.ToString("ddMMyy")).ToList();

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
                obj.Trans_no = "JPN_" + transdate.ToString("ddMMyy") + number;

                obj.entry_date = DateTime.Now;
                obj.update_date = DateTime.Now;
                obj.entry_user = User.Identity.Name;
                obj.update_user = User.Identity.Name;
                obj.flag_aktif = "1";
                var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
                obj.company_id = datas.COMPANY_ID;
                try
                {

                    db.JpnTbl.Add(obj);
                    db.SaveChanges();


                }
                catch (Exception ex)
                {
                    errormsg = ex.ToString();
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
                return Ok(new { message = errormsg });
            }
            else
            {

                return Ok(new { message = "Periode Sudah di closing" });

            }

        }
        #endregion datamanagement


        #region reportdata
        [Authorize]
        [HttpPost("GeneratePreviewTB")]
        public async Task<IActionResult> GeneratePreviewTB([FromBody] LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            // Render the "Index" view as a PDF
            if (isyearly)
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
                var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();


                var jpballdt = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejpb = jpballdt.TransDate.Year;

                var jpnalldt = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejpn = jpnalldt.TransDate.Year;

                var jmalldt = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejm = jmalldt.TransDate.Year;




                var datajpb = db.JpbTbl.Where(y => y.TransDate.Year >= startdatejpb && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => y.TransDate.Year >= startdatejpn && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => y.TransDate.Year >= startdatejm && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();

                obj.akundata = dataacc;
                obj.jpbdata = datajpb;
                obj.jpndata = datajpn;
                obj.jmdata = datajm;
                obj.ispreview = true;
                List<LRRptModel> rptdata = new List<LRRptModel>();

                foreach (var dt in dataacc)
                {
                    LRRptModel fld = new LRRptModel();
                    fld.akun = dt.account_no.ToString();
                    fld.description = dt.account_name;
                    fld.akundk = dt.akundk;
                    if (dt.akundk == "K")
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                    }
                    else
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                List<TBModel> TBData_D = new List<TBModel>();
                foreach (var dt in dataacc_D)
                {
                    TBModel fld = new TBModel();
                    fld.AccountNo_D = dt.account_no;
                    fld.Desc_D = dt.account_name;
                    fld.akundk_D = dt.akundk;
                    var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                    {
                        fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                        fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }
                    else
                    {
                        fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                    }
                    TBData_D.Add(fld);



                }

                List<TBModel> TBData_K = new List<TBModel>();
                foreach (var dt in dataacc_K)
                {
                    TBModel fld = new TBModel();
                    fld.AccountNo_K = dt.account_no;
                    fld.Desc_K = dt.account_name;
                    fld.akundk_K = dt.akundk;
                    var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);


                    fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                    fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                    TBData_K.Add(fld);



                }
                obj.TB_D = TBData_D;
                obj.TB_K = TBData_K;
                byte[] pdfBytes = GenerateTrialBalancePdf(obj);
                var FileName = "NeracaPreview" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);
            }
            else
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();

                var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();

                var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
                var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();


                var jpballdt = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejpb = jpballdt.TransDate.Year;
                var startmonthjpb = jpballdt.TransDate.Month;

                var jpnalldt = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejpn = jpnalldt.TransDate.Year;
                var startmonthjpn = jpnalldt.TransDate.Month;

                var jmalldt = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejm = jmalldt.TransDate.Year;
                var startmonthjm = jmalldt.TransDate.Month;




                var datajpb = db.JpbTbl.Where(y => (y.TransDate.Year > startdatejpb ||
                                 (y.TransDate.Year == startdatejpb && y.TransDate.Month >= startmonthjpb))
                                &&
                                (y.TransDate.Year < year ||
                                 (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => (y.TransDate.Year > startdatejpn ||
                                 (y.TransDate.Year == startdatejpn && y.TransDate.Month >= startmonthjpn))
                                &&
                                (y.TransDate.Year < year ||
                                 (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => (y.TransDate.Year > startdatejm ||
                                 (y.TransDate.Year == startdatejm && y.TransDate.Month >= startmonthjm))
                                &&
                                (y.TransDate.Year < year ||
                                 (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();

                obj.akundata = dataacc;
                obj.jpbdata = datajpb;
                obj.jpndata = datajpn;
                obj.jmdata = datajm;
                obj.ispreview = true;

                List<LRRptModel> rptdata = new List<LRRptModel>();

                foreach (var dt in dataacc)
                {
                    LRRptModel fld = new LRRptModel();
                    fld.akun = dt.account_no.ToString();
                    fld.description = dt.account_name;
                    fld.akundk = dt.akundk;
                    if (dt.akundk == "K")
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                    }
                    else
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                List<TBModel> TBData_D = new List<TBModel>();
                foreach (var dt in dataacc_D)
                {
                    TBModel fld = new TBModel();
                    fld.AccountNo_D = dt.account_no;
                    fld.Desc_D = dt.account_name;
                    fld.akundk_D = dt.akundk;
                    var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                    {
                        fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                        fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }
                    else
                    {
                        fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                    }
                    TBData_D.Add(fld);



                }

                List<TBModel> TBData_K = new List<TBModel>();
                foreach (var dt in dataacc_K)
                {
                    TBModel fld = new TBModel();
                    fld.AccountNo_K = dt.account_no;
                    fld.Desc_K = dt.account_name;
                    fld.akundk_K = dt.akundk;
                    var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);


                    fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                    fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                    TBData_K.Add(fld);



                }
                obj.TB_D = TBData_D;
                obj.TB_K = TBData_K;
                byte[] pdfBytes = GenerateTrialBalancePdf(obj);
                var FileName = "NeracaPreview" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);
            }
        }

        [Authorize]
        [HttpPost("GeneratePdfTB")]
        public async Task<IActionResult> GeneratePdfTB([FromBody] LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.isclosed == "N" && y.periode == month || String.IsNullOrEmpty(y.isclosed)).ToList();
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            // Render the "Index" view as a PDF

            if (dataclosing.Count == 0)
            {
                if (isyearly)
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();


                    var jpballdt = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejpb = jpballdt.TransDate.Year;

                    var jpnalldt = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejpn = jpnalldt.TransDate.Year;

                    var jmalldt = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejm = jmalldt.TransDate.Year;




                    var datajpb = db.JpbTbl.Where(y => y.TransDate.Year >= startdatejpb && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpn = db.JpnTbl.Where(y => y.TransDate.Year >= startdatejpn && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajm = db.JmTbl.Where(y => y.TransDate.Year >= startdatejm && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();

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
                        fld.akundk = dt.akundk;
                        if (dt.akundk == "K")
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                        }
                        else
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;
                    List<TBModel> TBData_D = new List<TBModel>();
                    foreach (var dt in dataacc_D)
                    {
                        TBModel fld = new TBModel();
                        fld.AccountNo_D = dt.account_no;
                        fld.Desc_D = dt.account_name;
                        fld.akundk_D = dt.akundk;
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                        {
                            fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                            fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }
                        else
                        {
                            fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                        }
                        TBData_D.Add(fld);



                    }

                    List<TBModel> TBData_K = new List<TBModel>();
                    foreach (var dt in dataacc_K)
                    {
                        TBModel fld = new TBModel();
                        fld.AccountNo_K = dt.account_no;
                        fld.Desc_K = dt.account_name;
                        fld.akundk_K = dt.akundk;
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);


                        fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                        TBData_K.Add(fld);



                    }
                    obj.TB_D = TBData_D;
                    obj.TB_K = TBData_K;
                    byte[] pdfBytes = GenerateTrialBalancePdf(obj);
                    var FileName = "Neraca" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                    return File(pdfBytes, "application/pdf", FileName);
                }
                else
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();

                    var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();

                    var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();


                    var jpballdt = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejpb = jpballdt.TransDate.Year;
                    var startmonthjpb = jpballdt.TransDate.Month;

                    var jpnalldt = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejpn = jpnalldt.TransDate.Year;
                    var startmonthjpn = jpnalldt.TransDate.Month;

                    var jmalldt = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejm = jmalldt.TransDate.Year;
                    var startmonthjm = jmalldt.TransDate.Month;




                    var datajpb = db.JpbTbl.Where(y => (y.TransDate.Year > startdatejpb ||
                                     (y.TransDate.Year == startdatejpb && y.TransDate.Month >= startmonthjpb))
                                    &&
                                    (y.TransDate.Year < year ||
                                     (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpn = db.JpnTbl.Where(y => (y.TransDate.Year > startdatejpn ||
                                     (y.TransDate.Year == startdatejpn && y.TransDate.Month >= startmonthjpn))
                                    &&
                                    (y.TransDate.Year < year ||
                                     (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();
                    var datajm = db.JmTbl.Where(y => (y.TransDate.Year > startdatejm ||
                                     (y.TransDate.Year == startdatejm && y.TransDate.Month >= startmonthjm))
                                    &&
                                    (y.TransDate.Year < year ||
                                     (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();

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
                        fld.akundk = dt.akundk;
                        if (dt.akundk == "K")
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                        }
                        else
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;
                    List<TBModel> TBData_D = new List<TBModel>();
                    foreach (var dt in dataacc_D)
                    {
                        TBModel fld = new TBModel();
                        fld.AccountNo_D = dt.account_no;
                        fld.Desc_D = dt.account_name;
                        fld.akundk_D = dt.akundk;
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                        {
                            fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                            fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }
                        else
                        {
                            fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                        }
                        TBData_D.Add(fld);



                    }

                    List<TBModel> TBData_K = new List<TBModel>();
                    foreach (var dt in dataacc_K)
                    {
                        TBModel fld = new TBModel();
                        fld.AccountNo_K = dt.account_no;
                        fld.Desc_K = dt.account_name;
                        fld.akundk_K = dt.akundk;
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);


                        fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                        TBData_K.Add(fld);



                    }
                    obj.TB_D = TBData_D;
                    obj.TB_K = TBData_K;
                    byte[] pdfBytes = GenerateTrialBalancePdf(obj);
                    var FileName = "Neraca" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                    return File(pdfBytes, "application/pdf", FileName);
                }

            }
            else
            {
                return Ok(new { message = "Periode belum di closing" });
            }
        }

        private byte[] GenerateTrialBalancePdf(LRModel obj)
        {
            using var stream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape()); // Set to Landscape
                    page.Margin(10); // Reduced margin
                    page.Header().Column(col =>
                    {
                        if (obj.ispreview == true)
                        {
                            col.Item().Text("Trial Balance Report (PREVIEW)")
                            .Bold()
                            .FontSize(12) // Reduced font size
                            .AlignCenter();
                        }
                        else
                        {
                            col.Item().Text("Trial Balance Report")
                              .Bold()
                              .FontSize(12) // Reduced font size
                              .AlignCenter();
                        }


                        col.Item().Text("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
                            .FontSize(8)
                            .AlignCenter();

                        col.Item().Text("Company XYZ - Financial Report")
                            .FontSize(8)
                            .AlignCenter();
                    });

                    page.Content().Table(table =>
                    {
                        // Define Columns for Side-by-Side Format
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);  // ID column (left)
                            columns.RelativeColumn(3);  // Account Name (left)
                            columns.RelativeColumn(1.5f);  // Debit (left)

                            columns.RelativeColumn(1);  // ID column (right)
                            columns.RelativeColumn(3);  // Account Name (right)
                            columns.RelativeColumn(1.5f);  // Credit (right)

                        });

                        // Header Row
                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(2).Text("Account_No").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Account Name").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Debit").Bold().FontSize(8);

                            header.Cell().Border(1).Padding(2).Text("Account_No").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Account Name").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Credit").Bold().FontSize(8);

                        });
                        var dt_d = obj.TB_D.Where(y => y.AccountNo_D != 0).OrderBy(y => y.AccountNo_D).ToList();
                        var dt_k = obj.TB_K.Where(y => y.AccountNo_K != 0).OrderBy(y => y.AccountNo_K).ToList();
                        var length = 0;
                        if (dt_d.Count() > dt_k.Count())
                        {
                            length = dt_d.Count() + 7;
                        }
                        else if (dt_d.Count() < dt_k.Count())
                        {
                            length = dt_k.Count() + 7;
                        }
                        string previousFirstDigit_D = null;

                        // Sample Data Rows - Side by Side
                        for (int i = 1; i <= length; i++)
                        {

                            if (i < dt_d.Count())
                            {

                                var fld_D = dt_d[i];
                                string currentFirstDigit_D = fld_D.AccountNo_D.ToString().Substring(0, 2);
                                var max = dt_d.Where(y => y.AccountNo_D.ToString().Substring(0, 2) == currentFirstDigit_D).Max(y => y.AccountNo_D);
                                var last2digitstr = max.ToString().Substring(max.ToString().Length - 2);
                                var last2digit = Convert.ToInt32(last2digitstr);
                                var currentlast2digit = fld_D.AccountNo_D.ToString().Substring(fld_D.AccountNo_D.ToString().Length - 2);
                                //if (previousFirstDigit_D != currentFirstDigit_D && previousFirstDigit_D != null)
                                //{
                                //    var subtotal = obj.ReportModel.Where(y => y.akun.StartsWith(previousFirstDigit_D)).Sum(y => y.totalint);
                                //    table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                //    table.Cell().Border(1).Padding(2).Text("Jumlah Aktiva Lancar").FontSize(7);
                                //    table.Cell().Border(1).Padding(2).Text("").FontSize(7).AlignRight();
                                //    table.Cell().Border(1).Padding(2).Text(subtotal.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight(); // Debit 2 Value
                                //}
                                table.Cell().Border(1).Padding(2).Text(fld_D.AccountNo_D).FontSize(7);
                                table.Cell().Border(1).Padding(2).Text(fld_D.Desc_D).FontSize(7);
                                if (fld_D.akundk_D == "-")
                                {
                                    table.Cell().Border(1).Padding(2).Text("").FontSize(7).AlignRight();

                                }
                                else
                                {
                                    table.Cell().Border(1).Padding(2).Text(fld_D.Value_D).FontSize(7).AlignRight();

                                }

                                previousFirstDigit_D = currentFirstDigit_D;

                            }
                            else
                            {
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7).AlignRight(); // Debit Value

                            }
                            if (i < dt_k.Count())
                            {
                                var fld_K = dt_k[i];
                                table.Cell().Border(1).Padding(2).Text(fld_K.AccountNo_K).FontSize(7);
                                table.Cell().Border(1).Padding(2).Text(fld_K.Desc_K).FontSize(7);
                                if (fld_K.akundk_K == "-")
                                {
                                    table.Cell().Border(1).Padding(2).Text("").FontSize(7).AlignRight();

                                }
                                else
                                {
                                    if (fld_K.AccountNo_K == 3000002)
                                    {
                                        var subtotal3 = obj.ReportModel.Sum(y => y.totalint);
                                        table.Cell().Border(1).Padding(2).Text(subtotal3.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();

                                    }
                                    else
                                    {
                                        table.Cell().Border(1).Padding(2).Text(fld_K.Value_K).FontSize(7).AlignRight();

                                    }

                                }
                            }
                            else
                            {
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7).AlignRight(); // Debit Value

                            }


                            //table.Cell().Border(1).Padding(2).Text(i.ToString()).FontSize(7);
                            //table.Cell().Border(1).Padding(2).Text($"Account {i}").FontSize(7);
                            //table.Cell().Border(1).Padding(2).Text((i * 1000).ToString("C2")).FontSize(7).AlignRight(); // Debit Value

                            //table.Cell().Border(1).Padding(2).Text((i + 5).ToString()).FontSize(7);
                            //table.Cell().Border(1).Padding(2).Text($"Account {i + 5}").FontSize(7);
                            //table.Cell().Border(1).Padding(2).Text((i % 2 == 0 ? (i * 800).ToString("C2") : "-")).FontSize(7).AlignRight(); // Credit Value
                        }
                        var subtotalequity = obj.ReportModel.Sum(y => y.totalint);

                        var subtotal_D = dt_d.Sum(y => y.Value_D_int);
                        var subtotal_K = dt_k.Sum(y => y.Value_K_int) + subtotalequity;


                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("Jumlah").Bold().FontSize(8);
                        table.Cell().Border(1).Padding(2).Text(subtotal_D.ToString("#,##0.00;(#,##0.00)")).Bold().FontSize(8).AlignRight(); // Debit Value
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("Jumlah").Bold().FontSize(8);
                        table.Cell().Border(1).Padding(2).Text(subtotal_K.ToString("#,##0.00;(#,##0.00)")).Bold().FontSize(8).AlignRight(); // Debit Value

                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Generated using QuestPDF").FontSize(7); // Smaller footer font
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }


        [Authorize]
        [HttpPost("GeneratePdfLR")]
        public async Task<IActionResult> GeneratePdfLR([FromBody] LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.periode >= 1 && y.periode <= month && y.isclosed == "Y").ToList();
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            // Render the "Index" view as a PDF

            if (dataclosing.Count == month)
            {
                if (isyearly)
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpb = db.JpbTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpn = db.JpnTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajm = db.JmTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    obj.akundata = dataacc;
                    obj.jpbdata = datajpb;
                    obj.jpndata = datajpn;
                    obj.jmdata = datajm;
                    obj.closingdata = dataclosing;
                    obj.ispreview = false;

                    List<LRRptModel> rptdata = new List<LRRptModel>();

                    foreach (var dt in dataacc)
                    {
                        LRRptModel fld = new LRRptModel();
                        fld.akun = dt.account_no.ToString();
                        fld.description = dt.account_name;
                        fld.akundk = dt.akundk;
                        if (dt.akundk == "K")
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                        }
                        else

                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;
                    byte[] pdfBytes = GeneratePdfV2(obj);
                    var FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                    return File(pdfBytes, "application/pdf", FileName);

                    //return new ViewAsPdf("LR_Rpt", obj)
                    //{
                    //    FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf",
                    //    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    //    PageMargins = new Rotativa.AspNetCore.Options.Margins
                    //    {
                    //        Left = 5,   // Set narrow margin for the left (in millimeters)
                    //        Right = 5,  // Set narrow margin for the right (in millimeters)
                    //        Top = 10,   // Set narrow margin for the top (in millimeters)
                    //        Bottom = 10 // Set narrow margin for the bottom (in millimeters)
                    //    }
                    //};
                }
                else
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpb = db.JpbTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpn = db.JpnTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajm = db.JmTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    obj.akundata = dataacc;
                    obj.jpbdata = datajpb;
                    obj.jpndata = datajpn;
                    obj.jmdata = datajm;
                    obj.closingdata = dataclosing;
                    obj.ispreview = false;

                    List<LRRptModel> rptdata = new List<LRRptModel>();

                    foreach (var dt in dataacc)
                    {
                        LRRptModel fld = new LRRptModel();
                        fld.akun = dt.account_no.ToString();
                        fld.description = dt.account_name;
                        fld.akundk = dt.akundk;

                        if (dt.akundk == "K")
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                        }
                        else

                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;

                    byte[] pdfBytes = GeneratePdfV2(obj);
                    var FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                    return File(pdfBytes, "application/pdf", FileName);

                    //return new ViewAsPdf("LR_Rpt", obj)
                    //{
                    //    FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf",
                    //    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    //    PageMargins = new Rotativa.AspNetCore.Options.Margins
                    //    {
                    //        Left = 5,   // Set narrow margin for the left (in millimeters)
                    //        Right = 5,  // Set narrow margin for the right (in millimeters)
                    //        Top = 10,   // Set narrow margin for the top (in millimeters)
                    //        Bottom = 10 // Set narrow margin for the bottom (in millimeters)
                    //    }
                    //};
                }

            }
            else
            {
                return Ok(new { message = "Periode belum di closing" });
            }

        }

        private byte[] GeneratePdfV2(LRModel obj)
        {
            using var stream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(10); // Reduced margin

                    page.Header().Column(col =>
                    {
                        if (obj.ispreview == true)
                        {
                            col.Item().Text("Report Laba Rugi (PREVIEW)")
                           .Bold()
                           .FontSize(12) // Reduced font size
                           .AlignCenter();
                        }
                        else
                        {
                            col.Item().Text("Report Laba Rugi")
                           .Bold()
                           .FontSize(12) // Reduced font size
                           .AlignCenter();
                        }

                        col.Item().Text($"Tahun: {obj.year}")
                            .FontSize(8)
                            .AlignCenter();

                        if (!obj.isYearly)
                        {
                            col.Item().Text($"Periode: {obj.month}")
                                .FontSize(8)
                                .AlignCenter();
                        }

                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(8)
                            .AlignCenter();
                        col.Item().Text("Company XYZ - Financial Report")
                            .FontSize(8)
                            .AlignCenter();
                    });

                    page.Content().Table(table =>
                    {
                        // Adjusted column widths
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1.5f); // Reduced Value column width
                            columns.RelativeColumn(1.5f); // Reduced Total column width
                        });

                        table.Header(header =>
                        {
                            header.Cell().Border(1).Padding(2).Text("Account No").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Description").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Value").Bold().FontSize(8);
                            header.Cell().Border(1).Padding(2).Text("Total").Bold().FontSize(8);
                        });

                        string previousFirstDigit = null;

                        foreach (var dt in obj.ReportModel)
                        {
                            string currentFirstDigit = dt.akun.Substring(0, 1);

                            // Add a subtotal row when a new category starts
                            if (previousFirstDigit != currentFirstDigit && previousFirstDigit != null)
                            {
                                var subtotal = obj.ReportModel.Where(y => y.akun.StartsWith(previousFirstDigit)).Sum(y => y.totalint);

                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text("Sub Total").FontSize(7).Bold();
                                table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                table.Cell().Border(1).Padding(2).Text(subtotal.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();

                                if (previousFirstDigit == "5")
                                {
                                    string[] codearr = { "4", "5" };
                                    var subtotal2 = obj.ReportModel.Where(y => codearr.Contains(y.akun.Substring(0, 1))).Sum(y => y.totalint);

                                    table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                    table.Cell().Border(1).Padding(2).Text("HPP").FontSize(7).Bold();
                                    table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                                    table.Cell().Border(1).Padding(2).Text(subtotal2.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();
                                }
                            }

                            table.Cell().Border(1).Padding(2).Text(dt.akun).FontSize(7);
                            table.Cell().Border(1).Padding(2).Text(dt.description).FontSize(7);
                            table.Cell().Border(1).Padding(2).Text(dt.akundk != "-" ? dt.total : "").FontSize(7).AlignRight();
                            table.Cell().Border(1).Padding(2).Text("").FontSize(7);

                            previousFirstDigit = currentFirstDigit;
                        }

                        var subtotal3 = obj.ReportModel.Sum(y => y.totalint);
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text("Laba Bersih Usaha").FontSize(7).Bold();
                        table.Cell().Border(1).Padding(2).Text("").FontSize(7);
                        table.Cell().Border(1).Padding(2).Text(subtotal3.ToString("#,##0.00;(#,##0.00)")).FontSize(7).AlignRight();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Generated using QuestPDF").FontSize(7); // Smaller footer font
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }


        [Authorize]
        [HttpPost("GeneratePreviewLR")]
        public async Task<IActionResult> GeneratePreviewLR([FromBody] LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.periode >= 1 && y.periode <= month && y.isclosed == "Y").ToList();
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            // Render the "Index" view as a PDF

            if (isyearly)
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var datajpb = db.JpbTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
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
                    fld.akundk = dt.akundk;
                    if (dt.akundk == "K")
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                    }
                    else

                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                obj.ispreview = true;
                byte[] pdfBytes = GeneratePdfV2(obj);
                var FileName = "LabaRugiPreview" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);


            }
            else
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var datajpb = db.JpbTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
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
                    fld.akundk = dt.akundk;

                    if (dt.akundk == "K")
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.totalint = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                    }
                    else

                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                obj.ispreview = true;
                byte[] pdfBytes = GeneratePdfV2(obj);
                var FileName = "LabaRugiPreview" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);

            }

        }


        [Authorize]
        [HttpPost("GeneratePreviewCashflow")]
        public async Task<IActionResult> GeneratePreviewCashflow([FromBody] LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.periode == month && y.isclosed == "Y").ToList();
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            var datatb = getdatatbpreview(obj);
            // Render the "Index" view as a PDF

            if (isyearly)
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var datajpb = db.JpbTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
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
                    fld.akundk = dt.akundk;
                    if (dt.akundk == "K")
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                    }
                    else

                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                obj.TB_D = datatb.TB_D;
                obj.TB_K = datatb.TB_K;
                obj.ispreview = true;
                byte[] pdfBytes = GenerateRptCashflow(obj);
                var FileName = "Cashflowpreview" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);


            }
            else
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var datajpb = db.JpbTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                obj.akundata = dataacc;
                obj.jpbdata = datajpb;
                obj.jpndata = datajpn;
                obj.jmdata = datajm;
                obj.closingdata = dataclosing;
                obj.ispreview = false;

                List<LRRptModel> rptdata = new List<LRRptModel>();

                foreach (var dt in dataacc)
                {
                    LRRptModel fld = new LRRptModel();
                    fld.akun = dt.account_no.ToString();
                    fld.description = dt.account_name;
                    fld.akundk = dt.akundk;

                    if (dt.akundk == "K")
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.totalint = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                    }
                    else

                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                obj.TB_D = datatb.TB_D;
                obj.TB_K = datatb.TB_K;
                byte[] pdfBytes = GenerateRptCashflow(obj);
                var FileName = "CashflowPreviewYtd" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                return File(pdfBytes, "application/pdf", FileName);

            }

        }

        [Authorize]
        [HttpPost("GeneratePdfCashflow")]
        public async Task<IActionResult> GeneratePdfCashflow([FromBody] LRModel obj)
        {
            var year = obj.year;
            var isyearly = obj.isYearly;
            var month = obj.month;
            if (isyearly)
            {
                month = 12;
            }
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.periode >= 1 && y.periode <= month && y.isclosed == "Y").ToList();
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            // Render the "Index" view as a PDF
            var datatb = getdatatbclosed(obj);
            if (dataclosing.Count == month)
            {
                if (isyearly)
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpb = db.JpbTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpn = db.JpnTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajm = db.JmTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    obj.akundata = dataacc;
                    obj.jpbdata = datajpb;
                    obj.jpndata = datajpn;
                    obj.jmdata = datajm;
                    obj.closingdata = dataclosing;
                    obj.ispreview = false;

                    List<LRRptModel> rptdata = new List<LRRptModel>();

                    foreach (var dt in dataacc)
                    {
                        LRRptModel fld = new LRRptModel();
                        fld.akun = dt.account_no.ToString();
                        fld.description = dt.account_name;
                        fld.akundk = dt.akundk;
                        if (dt.akundk == "K")
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                        }
                        else

                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;
                    obj.TB_D = datatb.TB_D;
                    obj.TB_K = datatb.TB_K;
                    byte[] pdfBytes = GenerateRptCashflow(obj);
                    var FileName = "Cashflow" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                    return File(pdfBytes, "application/pdf", FileName);

                    //return new ViewAsPdf("LR_Rpt", obj)
                    //{
                    //    FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf",
                    //    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    //    PageMargins = new Rotativa.AspNetCore.Options.Margins
                    //    {
                    //        Left = 5,   // Set narrow margin for the left (in millimeters)
                    //        Right = 5,  // Set narrow margin for the right (in millimeters)
                    //        Top = 10,   // Set narrow margin for the top (in millimeters)
                    //        Bottom = 10 // Set narrow margin for the bottom (in millimeters)
                    //    }
                    //};
                }
                else
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpb = db.JpbTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpn = db.JpnTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajm = db.JmTbl.Where(y => y.TransDate.Month >= 1 && y.TransDate.Month <= month && y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
                    obj.akundata = dataacc;
                    obj.jpbdata = datajpb;
                    obj.jpndata = datajpn;
                    obj.jmdata = datajm;
                    obj.closingdata = dataclosing;
                    obj.ispreview = false;

                    List<LRRptModel> rptdata = new List<LRRptModel>();

                    foreach (var dt in dataacc)
                    {
                        LRRptModel fld = new LRRptModel();
                        fld.akun = dt.account_no.ToString();
                        fld.description = dt.account_name;
                        fld.akundk = dt.akundk;

                        if (dt.akundk == "K")
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                        }
                        else

                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;
                    obj.TB_D = datatb.TB_D;
                    obj.TB_K = datatb.TB_K;
                    byte[] pdfBytes = GenerateRptCashflow(obj);
                    var FileName = "CashflowYtd" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf";
                    return File(pdfBytes, "application/pdf", FileName);

                    //return new ViewAsPdf("LR_Rpt", obj)
                    //{
                    //    FileName = "LabaRugi" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".pdf",
                    //    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    //    PageMargins = new Rotativa.AspNetCore.Options.Margins
                    //    {
                    //        Left = 5,   // Set narrow margin for the left (in millimeters)
                    //        Right = 5,  // Set narrow margin for the right (in millimeters)
                    //        Top = 10,   // Set narrow margin for the top (in millimeters)
                    //        Bottom = 10 // Set narrow margin for the bottom (in millimeters)
                    //    }
                    //};
                }

            }
            else
            {
                return Ok(new { message = "Periode belum di closing" });
            }

        }

        public LRModel getdatatbclosed(LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            var dataclosing = db.ClosingTbl.Where(y => y.year == year && y.isclosed == "N" && y.periode == month || String.IsNullOrEmpty(y.isclosed)).ToList();
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            // Render the "Index" view as a PDF

            if (dataclosing.Count == 0)
            {
                if (isyearly)
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();


                    var jpballdt = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejpb = jpballdt.TransDate.Year;

                    var jpnalldt = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejpn = jpnalldt.TransDate.Year;

                    var jmalldt = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejm = jmalldt.TransDate.Year;




                    var datajpb = db.JpbTbl.Where(y => y.TransDate.Year >= startdatejpb && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpn = db.JpnTbl.Where(y => y.TransDate.Year >= startdatejpn && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();
                    var datajm = db.JmTbl.Where(y => y.TransDate.Year >= startdatejm && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();

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
                        fld.akundk = dt.akundk;
                        if (dt.akundk == "K")
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                        }
                        else
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;
                    List<TBModel> TBData_D = new List<TBModel>();
                    foreach (var dt in dataacc_D)
                    {
                        TBModel fld = new TBModel();
                        fld.AccountNo_D = dt.account_no;
                        fld.Desc_D = dt.account_name;
                        fld.akundk_D = dt.akundk;
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                        {
                            fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                            fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }
                        else
                        {
                            fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                        }
                        TBData_D.Add(fld);



                    }

                    List<TBModel> TBData_K = new List<TBModel>();
                    foreach (var dt in dataacc_K)
                    {
                        TBModel fld = new TBModel();
                        fld.AccountNo_K = dt.account_no;
                        fld.Desc_K = dt.account_name;
                        fld.akundk_K = dt.akundk;
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);


                        fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                        TBData_K.Add(fld);



                    }
                    obj.TB_D = TBData_D;
                    obj.TB_K = TBData_K;

                }
                else
                {
                    var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();

                    var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();

                    var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
                    var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();


                    var jpballdt = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejpb = jpballdt.TransDate.Year;
                    var startmonthjpb = jpballdt.TransDate.Month;

                    var jpnalldt = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejpn = jpnalldt.TransDate.Year;
                    var startmonthjpn = jpnalldt.TransDate.Month;

                    var jmalldt = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                    var startdatejm = jmalldt.TransDate.Year;
                    var startmonthjm = jmalldt.TransDate.Month;




                    var datajpb = db.JpbTbl.Where(y => (y.TransDate.Year > startdatejpb ||
                                     (y.TransDate.Year == startdatejpb && y.TransDate.Month >= startmonthjpb))
                                    &&
                                    (y.TransDate.Year < year ||
                                     (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();
                    var datajpn = db.JpnTbl.Where(y => (y.TransDate.Year > startdatejpn ||
                                     (y.TransDate.Year == startdatejpn && y.TransDate.Month >= startmonthjpn))
                                    &&
                                    (y.TransDate.Year < year ||
                                     (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();
                    var datajm = db.JmTbl.Where(y => (y.TransDate.Year > startdatejm ||
                                     (y.TransDate.Year == startdatejm && y.TransDate.Month >= startmonthjm))
                                    &&
                                    (y.TransDate.Year < year ||
                                     (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();

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
                        fld.akundk = dt.akundk;
                        if (dt.akundk == "K")
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                        }
                        else
                        {
                            var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                            var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                            var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                            var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                            fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                            fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }


                        rptdata.Add(fld);
                    }
                    obj.ReportModel = rptdata;
                    List<TBModel> TBData_D = new List<TBModel>();
                    foreach (var dt in dataacc_D)
                    {
                        TBModel fld = new TBModel();
                        fld.AccountNo_D = dt.account_no;
                        fld.Desc_D = dt.account_name;
                        fld.akundk_D = dt.akundk;
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                        {
                            fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                            fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                        }
                        else
                        {
                            fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                            fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                        }
                        TBData_D.Add(fld);



                    }

                    List<TBModel> TBData_K = new List<TBModel>();
                    foreach (var dt in dataacc_K)
                    {
                        TBModel fld = new TBModel();
                        fld.AccountNo_K = dt.account_no;
                        fld.Desc_K = dt.account_name;
                        fld.akundk_K = dt.akundk;
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);


                        fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                        TBData_K.Add(fld);



                    }
                    obj.TB_D = TBData_D;
                    obj.TB_K = TBData_K;

                }

            }

            return obj;
        }

        public LRModel getdatatbpreview(LRModel obj)
        {
            var year = obj.year;
            var month = obj.month;
            var isyearly = obj.isYearly;
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.EnableDebugging = true;
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            // Render the "Index" view as a PDF
            if (isyearly)
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();
                var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
                var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();


                var jpballdt = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejpb = jpballdt.TransDate.Year;

                var jpnalldt = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejpn = jpnalldt.TransDate.Year;

                var jmalldt = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejm = jmalldt.TransDate.Year;




                var datajpb = db.JpbTbl.Where(y => y.TransDate.Year >= startdatejpb && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => y.TransDate.Year >= startdatejpn && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => y.TransDate.Year >= startdatejm && y.TransDate.Year <= year && y.company_id == datas.COMPANY_ID).ToList();

                obj.akundata = dataacc;
                obj.jpbdata = datajpb;
                obj.jpndata = datajpn;
                obj.jmdata = datajm;
                obj.ispreview = true;
                List<LRRptModel> rptdata = new List<LRRptModel>();

                foreach (var dt in dataacc)
                {
                    LRRptModel fld = new LRRptModel();
                    fld.akun = dt.account_no.ToString();
                    fld.description = dt.account_name;
                    fld.akundk = dt.akundk;
                    if (dt.akundk == "K")
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                    }
                    else
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                List<TBModel> TBData_D = new List<TBModel>();
                foreach (var dt in dataacc_D)
                {
                    TBModel fld = new TBModel();
                    fld.AccountNo_D = dt.account_no;
                    fld.Desc_D = dt.account_name;
                    fld.akundk_D = dt.akundk;
                    var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                    {
                        fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                        fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }
                    else
                    {
                        fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                    }
                    TBData_D.Add(fld);



                }

                List<TBModel> TBData_K = new List<TBModel>();
                foreach (var dt in dataacc_K)
                {
                    TBModel fld = new TBModel();
                    fld.AccountNo_K = dt.account_no;
                    fld.Desc_K = dt.account_name;
                    fld.akundk_K = dt.akundk;
                    var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);


                    fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                    fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                    TBData_K.Add(fld);



                }
                obj.TB_D = TBData_D;
                obj.TB_K = TBData_K;
            }
            else
            {
                var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();

                var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();

                var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
                var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();


                var jpballdt = db.JpbTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejpb = jpballdt.TransDate.Year;
                var startmonthjpb = jpballdt.TransDate.Month;

                var jpnalldt = db.JpnTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejpn = jpnalldt.TransDate.Year;
                var startmonthjpn = jpnalldt.TransDate.Month;

                var jmalldt = db.JmTbl.Where(y => y.company_id == datas.COMPANY_ID).ToList().OrderBy(y => y.TransDate).FirstOrDefault();
                var startdatejm = jmalldt.TransDate.Year;
                var startmonthjm = jmalldt.TransDate.Month;




                var datajpb = db.JpbTbl.Where(y => (y.TransDate.Year > startdatejpb ||
                                 (y.TransDate.Year == startdatejpb && y.TransDate.Month >= startmonthjpb))
                                &&
                                (y.TransDate.Year < year ||
                                 (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();
                var datajpn = db.JpnTbl.Where(y => (y.TransDate.Year > startdatejpn ||
                                 (y.TransDate.Year == startdatejpn && y.TransDate.Month >= startmonthjpn))
                                &&
                                (y.TransDate.Year < year ||
                                 (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();
                var datajm = db.JmTbl.Where(y => (y.TransDate.Year > startdatejm ||
                                 (y.TransDate.Year == startdatejm && y.TransDate.Month >= startmonthjm))
                                &&
                                (y.TransDate.Year < year ||
                                 (y.TransDate.Year == year && y.TransDate.Month <= month)) && y.company_id == datas.COMPANY_ID).ToList();

                obj.akundata = dataacc;
                obj.jpbdata = datajpb;
                obj.jpndata = datajpn;
                obj.jmdata = datajm;
                obj.ispreview = true;

                List<LRRptModel> rptdata = new List<LRRptModel>();

                foreach (var dt in dataacc)
                {
                    LRRptModel fld = new LRRptModel();
                    fld.akun = dt.account_no.ToString();
                    fld.description = dt.account_name;
                    fld.akundk = dt.akundk;
                    if (dt.akundk == "K")
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                    }
                    else
                    {
                        var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                        var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                        var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                        var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);

                        fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                        fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }


                    rptdata.Add(fld);
                }
                obj.ReportModel = rptdata;
                List<TBModel> TBData_D = new List<TBModel>();
                foreach (var dt in dataacc_D)
                {
                    TBModel fld = new TBModel();
                    fld.AccountNo_D = dt.account_no;
                    fld.Desc_D = dt.account_name;
                    fld.akundk_D = dt.akundk;
                    var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    if (dt.account_no >= 1200006 && dt.account_no <= 1200009)
                    {
                        fld.Value_D = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";

                        fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                    }
                    else
                    {
                        fld.Value_D = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                        fld.Value_D_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);


                    }
                    TBData_D.Add(fld);



                }

                List<TBModel> TBData_K = new List<TBModel>();
                foreach (var dt in dataacc_K)
                {
                    TBModel fld = new TBModel();
                    fld.AccountNo_K = dt.account_no;
                    fld.Desc_K = dt.account_name;
                    fld.akundk_K = dt.akundk;
                    var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Debit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => (long)y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => (long)y.Value_Disc);


                    fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                    fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                    TBData_K.Add(fld);



                }
                obj.TB_D = TBData_D;
                obj.TB_K = TBData_K;
            }
            return obj;
        }

        private byte[] GenerateRptCashflow(LRModel obj)
        {
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            var datalastyear = db.ClosingValueTbl.Where(y => y.company_id == datas.COMPANY_ID && y.year == obj.year - 1).ToList();
            int[] accpenyusutan = new int[] { 1200006, 1200007, 1200008, 1200009 };
            int[] accpiutang = new int[] { 1100006, 1100009, 1100012, 1100013, 1100014 };
            int[] acchutang = new int[] { 2100001, 2100002, 2100003, 2200001, 2200002, 2300001, 2300002, 2300003, 2300004, 2300005, };
            int[] accperalatan = new int[] { 1200003, 1200005 };

            var kasly = (long)0;
            if (datalastyear.Count() != 0)
            {
                var datalrly = datalastyear.Where(y => y.src == "LR").ToList();
                var dataTB_Dly = datalastyear.Where(y => y.src == "TB_D").ToList();
                var dataTB_Kly = datalastyear.Where(y => y.src == "TB_K").ToList();
                var creditly = datalrly.Sum(y => (long)y.credit);
                var debitly = datalrly.Sum(y => (long)y.debit);
                var saldolabaly = creditly + debitly;
                var penyusutanly = dataTB_Dly.Where(y => accpenyusutan.Contains(y.Akun_Debit)).Sum(y => (long)y.debit) * -1;
                var piutangly = dataTB_Dly.Where(y => accpiutang.Contains(y.Akun_Debit)).Sum(y => (long)y.debit) * -1;
                var hutangly = dataTB_Kly.Where(y => acchutang.Contains(y.Akun_Debit)).Sum(y => (long)y.debit);
                var kasbersihoperasily = saldolabaly + penyusutanly + piutangly + hutangly;
                var datapembelianperalatanly = datalastyear.Where(y => accperalatan.Contains(y.Akun_Debit)).Sum(y => (long)y.debit);
                var pembelianperalatannegately = datapembelianperalatanly * -1;
                var modally = dataTB_Kly.Where(y => y.Akun_Debit == 3000001).FirstOrDefault().debit;
                var kasbersihinventasily = datapembelianperalatanly + modally;
                kasly = kasbersihoperasily + pembelianperalatannegately + kasbersihinventasily;




            }

            var SaldoLaba = obj.ReportModel.Sum(y => y.totalint);
            var penyusutan = obj.TB_D.Where(y => accpenyusutan.Contains(y.AccountNo_D)).Sum(y => (long)y.Value_D_int) * -1;
            var piutang = obj.TB_D.Where(y => accpiutang.Contains(y.AccountNo_D)).Sum(y => (long)y.Value_D_int) * -1;
            var hutang = obj.TB_K.Where(y => acchutang.Contains(y.AccountNo_K)).Sum(y => (long)y.Value_K_int);
            var kasbersihoperasi = SaldoLaba + penyusutan + piutang + hutang;
            var datapembelianperalatan = obj.jmdata.Where(y => accperalatan.Contains(y.Akun_Debit)).Sum(y => y.Debit);
            var pembelianperalatannegate = datapembelianperalatan * -1;
            var modal = obj.TB_K.Where(y => y.AccountNo_K == 3000001).FirstOrDefault().Value_K_int;
            var kasbersihinventasi = datapembelianperalatan + modal;
            var penambahankas = kasbersihoperasi + pembelianperalatannegate + kasbersihinventasi;

            using var stream = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(10); // Reduced margin

                    page.Header().Column(col =>
                    {
                        if (obj.ispreview == true)
                        {
                            col.Item().Text("Report Arus Kas(PREVIEW)")
                           .Bold()
                           .FontSize(12) // Reduced font size
                           .AlignCenter();
                        }
                        else
                        {
                            col.Item().Text("Report Arus Kas")
                           .Bold()
                           .FontSize(12) // Reduced font size
                           .AlignCenter();
                        }

                        col.Item().Text($"Tahun: {obj.year}")
                            .FontSize(8)
                            .AlignCenter();

                        if (!obj.isYearly)
                        {
                            col.Item().Text($"Periode: {obj.month}")
                                .FontSize(8)
                                .AlignCenter();
                        }

                        col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                            .FontSize(8)
                            .AlignCenter();
                        col.Item().Text("Company XYZ - Financial Report")
                            .FontSize(8)
                            .AlignCenter();
                    });

                    page.Content().Table(table =>
                    {
                        // Define just 2 columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Description
                            columns.RelativeColumn(1); // Value
                        });

                        void AddRow(string desc, string value, bool bold = false)
                        {
                            table.Cell().Border(1).Padding(2).Text(desc).FontSize(7).Bold();
                            table.Cell().Border(1).Padding(2).Text(value).FontSize(7).AlignRight().Bold();
                        }

                        // Header
                        AddRow("Description", "Value", bold: true);

                        // Section: Operasi
                        AddRow("Arus Kas dari Aktivitas Operasi", "", bold: true);
                        AddRow("Saldo Laba", SaldoLaba.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Penyusutan", penyusutan.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Kenaikan Piutang", piutang.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Kenaikan Hutang", hutang.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Kas Bersih dari Aktivitas Operasi", kasbersihoperasi.ToString("#,##0.00;(#,##0.00)"), bold: true);

                        // Section: Investasi
                        AddRow("Arus Kas dari Aktivitas Investasi", "", bold: true);
                        AddRow("Penambahan Aset Tetap", pembelianperalatannegate.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Kas Bersih dari Aktivitas Investasi", pembelianperalatannegate.ToString("#,##0.00;(#,##0.00)"), bold: true);

                        // Section: Pembiayaan
                        AddRow("Arus Kas dari Aktivitas Pembiayaan", "", bold: true);
                        AddRow("Penambahan Aset Tetap", datapembelianperalatan.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Modal", modal.ToString("#,##0.00;(#,##0.00)"));
                        AddRow("Kas Bersih dari Aktivitas Pembiayaan", kasbersihinventasi.ToString("#,##0.00;(#,##0.00)"), bold: true);

                        // Section: Summary
                        AddRow("Penambahan Bersih Kas dan Bank", penambahankas.ToString("#,##0.00;(#,##0.00)"), bold: true);
                        AddRow("Kas dan Bank Awal Tahun", kasly.ToString("#,##0.00;(#,##0.00)"), bold: true); // Add value if available
                        AddRow("Kas dan Bank Akhir Tahun", penambahankas.ToString("#,##0.00;(#,##0.00)"), bold: true);
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Generated using QuestPDF").FontSize(7); // Smaller footer font
                });
            }).GeneratePdf(stream);

            return stream.ToArray();
        }
        #endregion reportdata

        #region viewdata
        [Authorize]
        [HttpPost("ViewJM")]
        public IActionResult ViewJM(int? id)
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
            
            return Json(fld);
        }

        [Authorize]
        [HttpPost("ViewJPB")]
        public IActionResult ViewJPB(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            dbJpb fld = db.JpbTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            
            return Json(fld);
        }

        [Authorize]
        [HttpPost("ViewJPN")]
        public IActionResult ViewJPN(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            dbJpn fld = db.JpnTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
           
            return Json(fld);
        }
        #endregion viewdata





    }

}
