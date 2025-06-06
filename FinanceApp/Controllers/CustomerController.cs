﻿using System;
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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using MimeDetective.Definitions;
using MimeDetective.Diagnostics;
using MimeDetective.Engine;
using MimeDetective.Storage;
using System.Net;
using BaseLineProject.Services;

namespace BaseLineProject.Controllers
{
    public class CustomerController : Controller
    {
        private readonly FormDBContext db;
        private IHostingEnvironment Environment;
        private readonly ILogger<CustomerController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        public IConfiguration Configuration { get; }
        private readonly IMailService mailService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public string EmailConfirmationUrl { get; set; }

        public CustomerController(FormDBContext db, ILogger<CustomerController> logger, RoleManager<IdentityRole> roleManager, IHostingEnvironment _environment, UserManager<IdentityUser> userManager, IConfiguration configuration, IMailService mailService)
        {
            _userManager = userManager;
            logger = logger;
            Environment = _environment;
            this.db = db;
            Configuration = configuration;
            _roleManager = roleManager;

            this.mailService = mailService;

        }
        [Authorize(Roles = "AccountAdmin")]
        public IActionResult Index()
        {
           
            var data = new List<dbCustomer>();
            var currentcompany = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            var companyid = currentcompany.COMPANY_ID;
            data = db.CustomerTbl.Where(y => y.COMPANY_ID == companyid).OrderBy(y => y.id).ToList();
         
            var counttotaluser = db.CustomerTbl.Where(y => y.COMPANY_ID == companyid).Count();
            var isallowed = true;
            if (currentcompany.VA1NOTE == "Basic")
            {
                return NotFound();
            } else if (currentcompany.VA1NOTE == "UMKM")
            {
                if(counttotaluser >= 3)
                {
                    isallowed = false;
                }
            }  else if (currentcompany.VA1NOTE == "Enterprise")
            {
                var val = Convert.ToInt32(currentcompany.VA1);
                if(counttotaluser >= val)
                {
                    isallowed = false;
                }
            }
            ViewData["isallowed"] = isallowed;

            return View(data);
        }

        [Authorize(Roles = "SuperAdmin")]
        public IActionResult ActivateUser()
        {

            var data = new List<dbCustomer>();
            List<IdentityUser> users = _userManager.Users.Where(y => y.EmailConfirmed == false).ToList();
            List<string> usrlist = new List<string>();
            foreach(var dt in users)
            {
                usrlist.Add(dt.UserName);
            }
            data = db.CustomerTbl.Where(y => usrlist.Contains(y.Email)).OrderBy(y => y.id).ToList();
            return View(data);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            var currentcompany = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            if(currentcompany.VA1NOTE == "Basic")
            {
                return NotFound();
            }

            return View();
        }
        [Authorize(Roles = "AccountAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind] dbCustomer objCust)
        {
            if (ModelState.IsValid)
            {
                int validate = 0;
                objCust.ENTRY_DATE = DateTime.Now;
                objCust.UPDATE_DATE = DateTime.Now;
                objCust.ENTRY_USER = User.Identity.Name;
                objCust.UPDATE_USER = User.Identity.Name;
                objCust.REG_DATE = DateTime.Now;
                objCust.FLAG_AKTIF = "1";
                var currentcompany = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
                objCust.COMPANY = currentcompany.COMPANY;
                objCust.COMPANY_ID = currentcompany.COMPANY_ID;
                objCust.VA1NOTE = currentcompany.VA1NOTE;

                int totaluser = 0;
                if(currentcompany.VA1NOTE == "Enterprise")
                {
                    totaluser = Convert.ToInt32(currentcompany.VA1);
                }
                else if(currentcompany.VA1NOTE == "UMKM")
                {
                    totaluser = 3;
                }
                else if(currentcompany.VA1NOTE == "Basic")
                {
                    totaluser = 1;
                }
                var counttotaluser = db.CustomerTbl.Where(y => y.COMPANY_ID == currentcompany.COMPANY_ID).Count();
                if (counttotaluser < totaluser)
                {
                    try
                    {

                        var user = new IdentityUser { UserName = objCust.Email.Trim(), Email = objCust.Email.Trim() };
                        var validateisexist = _userManager.FindByEmailAsync(objCust.Email.Trim());

                        if (validateisexist.Result == null)
                        {
                            var res = await UserSetup(user, objCust.Password);
                            db.CustomerTbl.Add(objCust);
                            db.SaveChanges();

                            SendWelcomeMail(objCust.Email.Trim());

                        }
                        else
                        {
                            validate = 1;
                        }


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
                    if (validate == 1)
                    {
                        objCust.errmsg = "This account is exist";
                        return View(objCust);
                    }
                    else
                    {
                        return RedirectToAction("Index");

                    }
                }
                else
                {
                    objCust.errmsg = "total user sudah maksimal";
                    return View(objCust);
                }
                                
            }
            return View(objCust);
        }
        public async Task<bool> UserSetup(IdentityUser user, string userpass)
        {
            if (user == null)
            {
                return false; // User object is null
            }

            // Step 1: Create user
            var createResult = await _userManager.CreateAsync(user, userpass);
            if (!createResult.Succeeded)
            {
                return false; // User creation failed
            }

            // Step 2: Ensure role exists before adding user to role
            var roleExists = await _roleManager.RoleExistsAsync("Accounting");
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("Accounting")); // Create role if not exists
            }

            // Step 3: Assign role to user
            var addToRoleResult = await _userManager.AddToRoleAsync(user, "Accounting");
            if (!addToRoleResult.Succeeded)
            {
                return false; // Role assignment failed
            }

            // Step 4: Confirm email and update user
            user.EmailConfirmed = true;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return false; // Email confirmation update failed
            }

            return true; // All operations succeeded
        }
         
        public async Task<bool> addrole(IdentityUser user)
        {
            var success = false;
            var result1 = await _userManager.AddToRoleAsync(user, "Accounting");
            if (result1.Succeeded)
            {
                success = true;
            }
            return success;
        }
        [Authorize(Roles = "AccountAdmin")]
        public IActionResult Edit(int id)
        {
            //if (string.IsNullOrEmpty(id))
            //{
            //    return NotFound();
            //}
            var currentcompany = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            if (currentcompany.VA1NOTE == "Basic")
            {
                return NotFound();
            }
            dbCustomer fld = db.CustomerTbl.Find(id);
            
            if (fld == null)
            {
                return NotFound();
            }

            return View(fld);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public  IActionResult Edit(int id, [Bind] dbCustomer fld)
        {
            
            if (ModelState.IsValid)
            {
                var editFld = db.CustomerTbl.Find(id);
                editFld.CUST_NAME = fld.CUST_NAME;
                if (User.IsInRole("AccountAdmin"))
                {
                    editFld.COMPANY = fld.COMPANY;
                    var findallaccountrelated = db.CustomerTbl.Where(y => y.COMPANY_ID == editFld.COMPANY_ID).ToList();
                    foreach(var col in findallaccountrelated)
                    {
                        col.COMPANY = fld.COMPANY;
                    }
                }
                editFld.PHONE1 = fld.PHONE1;

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
       
        private string GetContentType(string exts)
        {
            var types = GetMimeTypes();
            var ext = exts.ToLowerInvariant();
            return types[ext];
        }
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {"txt", "text/plain"},
                {"pdf", "application/pdf"},
                {"doc", "application/vnd.ms-word"},
                {"docx", "application/vnd.ms-word"},
                {"xls", "application/vnd.ms-excel"},
                {"xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
                {"png", "image/png"},
                {"jpg", "image/jpeg"},
                {"jpeg", "image/jpeg"},
                {"gif", "image/gif"},
                {"csv", "text/csv"}
            };
        }

        [Authorize(Roles = "AccountAdmin")]
        public IActionResult Delete(int id)
        {
            var currentcompany = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            if (currentcompany.VA1NOTE == "Basic")
            {
                return NotFound();
            }
            //if (string.IsNullOrEmpty(id))
            //{
            //    return NotFound();
            //}
            dbCustomer fld = db.CustomerTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            return View(fld);
        }
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            dbCustomer fld = db.CustomerTbl.Find(id);

            if (fld == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    //db.trainerDb.Remove(fld);
                    fld.FLAG_AKTIF = "0";
                    fld.UPDATE_DATE = DateTime.Now;
                    fld.UPDATE_USER = User.Identity.Name;
                    var test = await DeactivateAccount(fld.Email);
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
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "AccountAdmin")]
        public IActionResult ActivateAcc(int id)
        {
            //if (string.IsNullOrEmpty(id))
            //{
            //    return NotFound();
            //}
            var currentcompany = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
            if (currentcompany.VA1NOTE == "Basic")
            {
                return NotFound();
            }
            dbCustomer fld = db.CustomerTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            return View(fld);
        }

        [Authorize(Roles = "SuperAdmin")]
        public IActionResult ActivateAccAdmin(int id)
        {
            //if (string.IsNullOrEmpty(id))
            //{
            //    return NotFound();
            //}
            dbCustomer fld = db.CustomerTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            if(fld.VA1NOTE == "Enterprise")
            {
                fld.VA1 = "1";
            }
            return View(fld);
        }


        [HttpPost, ActionName("ActivateAcc")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateId(int id)
        {
            dbCustomer fld = db.CustomerTbl.Find(id);

            if (fld == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    //db.trainerDb.Remove(fld);
                    fld.FLAG_AKTIF = "1";
                    fld.UPDATE_DATE = DateTime.Now;
                    fld.UPDATE_USER = User.Identity.Name;
                    var test = await ActivateAccount(fld.Email);
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
            }
            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("ActivateAccAdmin")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateIdAdmin([Bind] dbCustomer flds)
        {
            dbCustomer fld = db.CustomerTbl.Find(flds.id);

            if (fld == null)
            {
                return NotFound();
            }
            else
            {
                try
                {
                    //db.trainerDb.Remove(fld);
                    fld.FLAG_AKTIF = "1";
                    if (fld.VA1NOTE == "Enterprise")
                    {
                        fld.VA1 = flds.VA1;
                    }
                    fld.UPDATE_DATE = DateTime.Now;
                    fld.UPDATE_USER = User.Identity.Name;
                    var test = await ActivateAccount(fld.Email);
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
            }
            return RedirectToAction("ActivateUser");
        }

        public async Task<bool> ActivateAccount(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false; // User not found
            }

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }
        public async Task<bool> DeactivateAccount(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false; // User not found
            }

            user.EmailConfirmed = false;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }
        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            EmailConfirmationUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                protocol: Request.Scheme);

            return Json("oke");
        }

        public string FtpUplTest()
        {
            string link = Configuration.GetConnectionString("LinkFTP");
            string user = Configuration.GetConnectionString("UserFTP");
            string pass = Configuration.GetConnectionString("PassFTP");
            var fileUrl = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\CreateStore");
            var files = Path.Combine(fileUrl, "RM_07_71001_300520221611.dat");
            var linkftp = "ftp://" + link;
            string relativePath = "/data7/trdataj";
            Uri serverUri = new Uri(linkftp);
            Uri relativeUri = new Uri(relativePath, UriKind.Relative);
            Uri fullUri = new Uri(serverUri, relativeUri);
            string PureFileName = new FileInfo(files).Name;

            String uploadUrl = String.Format("{0}/{1}/{2}", linkftp, "/data7/trdataj", PureFileName);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uploadUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            // This example assumes the FTP site uses anonymous logon.  
            request.Credentials = new NetworkCredential(user, pass);
            request.Proxy = null;
            request.KeepAlive = true;
            request.UseBinary = true;
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // Copy the contents of the file to the request stream.  
            StreamReader sourceStream = new StreamReader(files);
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            return "Upload File Complete, status " + response.StatusDescription;
        }
        public async void SendWelcomeMail(string Email)
        {
            var request = new WelcomeRequest();
            request.UserName = Email;
            request.ToEmail = Email;
           
            //request.ToEmail = Input.Email;
            try
            {
                await mailService.SendWelcomeEmailAsync(request);
                //return Ok();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async void SendVerifyEmail(string Email)
        {
            //string Email = "aditya.tresnaprana@bata.com";
            var request = new WelcomeRequest();
            request.UserName = Email;
            request.ToEmail = Email;
            //request.ToEmail = Input.Email;
            

           
            try
            {
                await mailService.SendVerifyEmailAsync(request);
                //return Ok();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public int getwk()
        {
            int week = 0;
            string mySqlConnectionStr = Configuration.GetConnectionString("ConnDataMart");

            using (MySqlConnection conn = new MySqlConnection(mySqlConnectionStr))
            {
                conn.Open();
                string query = @"select week(now(), 5)+1 as wk";

                MySqlCommand cmd = new MySqlCommand(query, conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        week = Convert.ToInt32(reader["wk"]);

                    }
                }
                conn.Close();
            }
            return week;
        }
        public async void TestSendWelcomeMail()
        {
            string Email = "customerbatatest5@mail.com";
            var request = new WelcomeRequest();
            request.UserName = Email;
            request.ToEmail = Email;
            //request.ToEmail = Input.Email;
            try
            {
                await mailService.SendWelcomeEmailAsync(request);
                //return Ok();
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
        }
        public async void TestSendVerifyEmail()
        {

            string Email = "customerbatatest8@mail.com";
            var request = new WelcomeRequest();
            request.UserName = Email;
            request.ToEmail = Email;
            //request.ToEmail = Input.Email;
            var fileUrl = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot");
            var files = Path.Combine(fileUrl, "FileCodeofConduct.pdf");
            List<IFormFile> fileList = new List<IFormFile>();


            using (var stream = System.IO.File.OpenRead(files))
            {
                var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));


                fileList.Add(file);

                request.Attachments = fileList;
            }
            try
            {
                await mailService.SendVerifyEmailAsync(request);
                //return Ok();
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
        }
    }
}
