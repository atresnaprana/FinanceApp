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

            var dataclosing = db.ClosingTbl.Where(y => y.datefrom <= obj.TransDate && y.dateto >= obj.TransDate && y.isclosed == "Y").ToList();
            if (dataclosing.Count() == 0)
            {
                var transdate = obj.TransDate;

                var errormsg = "Transaksi sudah di simpan";
                var existingsales = db.JmTbl.ToList();
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

            var dataclosing = db.ClosingTbl.Where(y => y.datefrom <= obj.TransDate && y.dateto >= obj.TransDate && y.isclosed == "Y").ToList();
            if (dataclosing.Count() == 0)
            {
                var errormsg = "Transaksi sudah di simpan";

                var transdate = obj.TransDate;

                var existingsales = db.JpbTbl.ToList();
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

            var dataclosing = db.ClosingTbl.Where(y => y.datefrom <= obj.TransDate && y.dateto >= obj.TransDate && y.isclosed == "Y").ToList();
            if (dataclosing.Count() == 0)
            {
                var errormsg = "Transaksi sudah di simpan";

                var transdate = obj.TransDate;

                var existingsales = db.JpnTbl.ToList();
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



    }

}
