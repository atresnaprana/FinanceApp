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
using FinanceApp.Services;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FinanceApp.Controllers
{
    public class ClosingController : Controller
    {
        private readonly FormDBContext db;
        public const string SessionKeyName = "TransDateStrJm";
        private readonly TaxEligibilityService _taxEligibilityService;
        private const decimal UMKM_LIMIT = 4_800_000_000m;
        private IHostingEnvironment Environment;
        private readonly ILogger<ClosingController> _logger;

        public ClosingController(FormDBContext db, ILogger<ClosingController> logger, IHostingEnvironment _environment, TaxEligibilityService taxEligibilityService)
        {
            logger = logger;
            Environment = _environment;
            this.db = db;
            _taxEligibilityService = taxEligibilityService;
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
                var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
                obj.company_id = datas.COMPANY_ID;
                if (obj.isclosebool)
                {
                    obj.isclosed = "Y";
                    if(obj.periode == 12)
                    {
                        var datalr = getdatalabarugi(obj.year);
                        var datatb = getdatatb(obj.year);
                        var labaditahan = datalr.Sum(y => y.totalint);
                        foreach(var fld in datalr)
                        {
                            var dt =new  dbClosedValue();
                            dt.src = "LR";
                            dt.entry_date = DateTime.Now;
                            dt.update_date = DateTime.Now;
                            dt.entry_user = User.Identity.Name;
                            dt.update_user = User.Identity.Name;
                            dt.company_id = datas.COMPANY_ID;
                            dt.year = obj.year;
                            if (fld.akundk == "K")
                            {
                                dt.Akun_Credit = Convert.ToInt32(fld.akun);
                                dt.credit = Convert.ToInt64(fld.totalint);
                                
                            }
                            else
                            {
                                dt.Akun_Debit = Convert.ToInt32(fld.akun);
                                dt.debit = Convert.ToInt64(fld.totalint);
                            }
                            db.ClosingValueTbl.Add(dt);

                        }

                        foreach (var fld in datatb.TB_D)
                        {
                            var dt = new dbClosedValue();
                            dt.src = "TB_D";
                            dt.entry_date = DateTime.Now;
                            dt.update_date = DateTime.Now;
                            dt.entry_user = User.Identity.Name;
                            dt.update_user = User.Identity.Name;
                            dt.company_id = datas.COMPANY_ID;
                            dt.year = obj.year;
                            dt.Akun_Debit = fld.AccountNo_D;
                            dt.debit = Convert.ToInt64(fld.Value_D_int);
                            db.ClosingValueTbl.Add(dt);

                        }

                        foreach (var fld in datatb.TB_K)
                        {
                            var dt = new dbClosedValue();
                            dt.src = "TB_K";
                            dt.entry_date = DateTime.Now;
                            dt.update_date = DateTime.Now;
                            dt.entry_user = User.Identity.Name;
                            dt.update_user = User.Identity.Name;
                            dt.company_id = datas.COMPANY_ID;
                            dt.year = obj.year;
                            dt.Akun_Debit = fld.AccountNo_K;
                            dt.debit = Convert.ToInt64(fld.Value_K_int);
                            db.ClosingValueTbl.Add(dt);
                        }
                        char incomeacc = '4';

                        var incomedata = datalr.Where(y => y.akun.FirstOrDefault() == incomeacc).ToList();
                        var fldld = new dbLd();
                        fldld.value = datalr.Sum(y => (long)y.totalint);
                        fldld.company_id = datas.COMPANY_ID;
                        fldld.year = obj.year;
                        fldld.entry_user = User.Identity.Name;
                        fldld.update_user = User.Identity.Name;
                        fldld.entry_date = DateTime.Now;
                        fldld.update_date = DateTime.Now;
                        db.LDTbl.Add(fldld);
                        var warningflag = incomedata.Sum(y => y.totalint) <= UMKM_LIMIT && datas.taxflagpercentage == "Y";
                        if (warningflag)
                        {
                            TempData["AlertMessage"] = "Omset sudah melebihi batas maksimal pajak UMKM, status eligibilitas di ubah untuk pajak non final";
                            var datacompany = db.CustomerTbl.Where(y => y.COMPANY_ID == datas.COMPANY_ID).ToList();
                            foreach(var fldz in datacompany)
                            {
                                fldz.taxflagpercentage = "N";
                            }
                        }
                       

                        _taxEligibilityService.CalculateAnnualTaxEligibility(
                              datas.COMPANY_ID,
                              obj.year
                        );


                    }
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

        public List<LRRptModel> getdatalabarugi(int year){
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
            var datajpb = db.JpbTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
            var datajpn = db.JpnTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
            var datajm = db.JmTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
            

            List<LRRptModel> rptdata = new List<LRRptModel>();

            foreach (var dt in dataacc)
            {
                LRRptModel fld = new LRRptModel();
                fld.akun = dt.account_no.ToString();
                fld.description = dt.account_name;
                fld.akundk = dt.akundk;
                if (dt.akundk == "K")
                {
                    var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                    fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                    fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                }
                else

                {
                    var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);

                    fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                    fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                }


                rptdata.Add(fld);
            }
            return rptdata;
        }

        public dbNeraca getdatatb(int year)
        {
            var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

            dbNeraca rptdatas = new dbNeraca();
            var dataacc = db.AccountTbl.Where(y => y.account_no >= 4000000 && y.company_id == datas.COMPANY_ID).ToList();
            var dataacc_TB = db.AccountTbl.Where(y => y.account_no < 4000000 && y.company_id == datas.COMPANY_ID).ToList();
            var dataacc_K = dataacc_TB.Where(y => y.account_no >= 2000000 && y.company_id == datas.COMPANY_ID).ToList();
            var dataacc_D = dataacc_TB.Where(y => y.account_no < 2000000 && y.company_id == datas.COMPANY_ID).ToList();

            var datajpb = db.JpbTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
            var datajpn = db.JpnTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
            var datajm = db.JmTbl.Where(y => y.TransDate.Year == year && y.company_id == datas.COMPANY_ID).ToList();
           
            List<LRRptModel> rptdata = new List<LRRptModel>();

            foreach (var dt in dataacc)
            {
                LRRptModel fld = new LRRptModel();
                fld.akun = dt.account_no.ToString();
                fld.description = dt.account_name;
                fld.akundk = dt.akundk;
                if (dt.akundk == "K")
                {
                    var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                    fld.total = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                    fld.totalint = totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc;


                }
                else
                {
                    var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                    var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                    var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit);
                    var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);
                    var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);

                    fld.total = "(" + (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00") + ")";
                    fld.totalint = -1 * (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);

                }


                rptdata.Add(fld);
            }
            List<TBModel> TBData_D = new List<TBModel>();
            foreach (var dt in dataacc_D)
            {
                TBModel fld = new TBModel();
                fld.AccountNo_D = dt.account_no;
                fld.Desc_D = dt.account_name;
                fld.akundk_D = dt.akundk;
                var totaljpb = datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value) - datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value);
                var totaljpn = datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value);
                var totaljm = datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit) - datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit);
                var totaljpndisc = datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
                var totaljpbdisc = datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc);
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
                var totaljpb = datajpb.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Value) - datajpb.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Value);
                var totaljpn = datajpn.Where(y => y.Akun_Credit == dt.account_no).Sum(y => (long)y.Value) - datajpn.Where(y => y.Akun_Debit == dt.account_no).Sum(y => (long)y.Value);
                var totaljm = datajm.Where(y => y.Akun_Credit == dt.account_no).Sum(y => y.Credit) - datajm.Where(y => y.Akun_Debit == dt.account_no).Sum(y => y.Debit);
                var totaljpndisc = datajpn.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpn.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);
                var totaljpbdisc = datajpb.Where(y => y.Akun_Credit_disc == dt.account_no).Sum(y => y.Value_Disc) - datajpb.Where(y => y.Akun_Debit_disc == dt.account_no).Sum(y => y.Value_Disc);


                fld.Value_K = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc).ToString("#,##0.00");
                fld.Value_K_int = (totaljpb + totaljpn + totaljm + totaljpndisc + totaljpbdisc);
                TBData_K.Add(fld);



            }
            rptdatas.TB_D = TBData_D;
            rptdatas.TB_K = TBData_K;
            return rptdatas;
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
                var datas = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();

                if (fld.isclosebool)
                {
                    editFld.isclosed = "Y";
                    if (fld.periode == 12)
                    {
                        var datalr = getdatalabarugi(fld.year);
                        var datatb = getdatatb(fld.year);
                        var labaditahan = datalr.Sum(y => y.totalint);
                        foreach (var flds in datalr)
                        {
                            var dt = new dbClosedValue();
                            dt.src = "LR";
                            dt.entry_date = DateTime.Now;
                            dt.update_date = DateTime.Now;
                            dt.entry_user = User.Identity.Name;
                            dt.update_user = User.Identity.Name;
                            dt.company_id = datas.COMPANY_ID;
                            dt.year = fld.year;
                            if (flds.akundk == "K")
                            {
                                dt.Akun_Credit = Convert.ToInt32(flds.akun);
                                dt.credit = Convert.ToInt32(flds.totalint);

                            }
                            else
                            {
                                dt.Akun_Debit = Convert.ToInt32(flds.akun);
                                dt.debit = Convert.ToInt32(flds.totalint);
                            }
                            db.ClosingValueTbl.Add(dt);

                        }

                        foreach (var flds in datatb.TB_D)
                        {
                            var dt = new dbClosedValue();
                            dt.src = "TB_D";
                            dt.entry_date = DateTime.Now;
                            dt.update_date = DateTime.Now;
                            dt.entry_user = User.Identity.Name;
                            dt.update_user = User.Identity.Name;
                            dt.company_id = datas.COMPANY_ID;
                            dt.year = fld.year;
                            dt.Akun_Debit = flds.AccountNo_D;
                            dt.debit = Convert.ToInt32(flds.Value_D_int);
                            db.ClosingValueTbl.Add(dt);

                        }

                        foreach (var flds in datatb.TB_K)
                        {
                            var dt = new dbClosedValue();
                            dt.src = "TB_K";
                            dt.entry_date = DateTime.Now;
                            dt.update_date = DateTime.Now;
                            dt.entry_user = User.Identity.Name;
                            dt.update_user = User.Identity.Name;
                            dt.company_id = datas.COMPANY_ID;
                            dt.year = fld.year;
                            dt.Akun_Debit = flds.AccountNo_K;
                            dt.debit = Convert.ToInt32(flds.Value_K_int);
                            db.ClosingValueTbl.Add(dt);
                        }
                        char incomeacc = '4';

                        var incomedata = datalr.Where(y => y.akun.FirstOrDefault() == incomeacc).ToList();
                        var fldld = new dbLd();
                        fldld.value = datalr.Sum(y => (long)y.totalint);
                        fldld.company_id = datas.COMPANY_ID;
                        fldld.year = fld.year;
                        fldld.entry_user = User.Identity.Name;
                        fldld.update_user = User.Identity.Name;
                        fldld.entry_date = DateTime.Now;
                        fldld.update_date = DateTime.Now;
                        db.LDTbl.Add(fldld);
                        var warningflag = incomedata.Sum(y => y.totalint) <= UMKM_LIMIT && datas.taxflagpercentage == "Y";
                        if (warningflag)
                        {
                            TempData["AlertMessage"] = "Omset sudah melebihi batas maksimal pajak UMKM, status eligibilitas di ubah untuk pajak non final";

                            var datacompany = db.CustomerTbl.Where(y => y.COMPANY_ID == datas.COMPANY_ID).ToList();
                            foreach (var fldz in datacompany)
                            {
                                fldz.taxflagpercentage = "N";
                            }
                        }


                        _taxEligibilityService.CalculateAnnualTaxEligibility(
                              datas.COMPANY_ID,
                              fld.year
                        );


                    }

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
