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
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Index()
        {
           
            var data = new List<dbCustomer>();
            data = db.CustomerTbl.Where(y => y.FLAG_AKTIF == "1").OrderBy(y => y.id).ToList();
            return View(data);
        }
        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {

            return View();
        }
        [Authorize(Roles = "SuperAdmin")]
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

                try
                {
                    //var user = new IdentityUser { UserName = objCust.Email.Trim(), Email = objCust.Email.Trim(), EmailConfirmed = true };

                    var user = new IdentityUser { UserName = objCust.Email.Trim(), Email = objCust.Email.Trim() };
                    var validateisexist = _userManager.FindByEmailAsync(objCust.Email.Trim());

                    if (validateisexist.Result == null)
                    {
                        //createuser(user, objCust.Password);
                        var res = await UserSetup(user, objCust.Password);                        
                        db.CustomerTbl.Add(objCust);
                        db.SaveChanges();
                        //var getuser = _userManager.FindByEmailAsync(objCust.Email.Trim());
                        //IdentityUser userdata = getuser.Result;
                        //ConfirmEmailAsync(objCust.Email.Trim());
                        //ApplyUserSettings(userdata);
                        SendWelcomeMail(objCust.Email.Trim());
                        //addrole(userdata);
                        //string mySqlConnectionStr = Configuration.GetConnectionString("DefaultConnection");

                        //using (MySqlConnection conn = new MySqlConnection(mySqlConnectionStr))
                        //{
                        //    conn.Open();
                        //    string query = "UPDATE AspNetUsers SET EmailConfirmed = '1' WHERE UserName = @UserName";

                        //    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        //    {
                        //        cmd.Parameters.AddWithValue("@UserName", objCust.Email.Trim());

                        //        int rowsAffected = cmd.ExecuteNonQuery(); // Executes the update
                        //        if (rowsAffected == 0)
                        //        {
                        //            Console.WriteLine("No records were updated. Check if the username exists.");
                        //        }
                        //    }
                        //}


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
                //apprDal.AddApproval(objApproval);
                if(validate == 1)
                {
                    objCust.errmsg = "This account is exist";
                    return View(objCust);
                }
                else
                {
                    return RedirectToAction("Index");

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
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Edit(int id)
        {
            //if (string.IsNullOrEmpty(id))
            //{
            //    return NotFound();
            //}

            dbCustomer fld = db.CustomerTbl.Find(id);
            if(fld.BL_FLAG == "1")
            {
                fld.isBlackList = true;
            }
            else
            {
                fld.isBlackList = false;

            }
            if (fld.isApproved == "1")
            {
                fld.isApproveBool = true;
            }
            else
            {
                fld.isApproveBool = false;
            }
            if (fld.isApproved2 == "1")
            {
                fld.isApproveBool2 = true;
            }
            else
            {
                fld.isApproveBool2 = false;
            }
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
            //if (id == null)
            //{
            //    return NotFound();
            //}
            if (ModelState.IsValid)
            {
                var editFld = db.CustomerTbl.Find(id);
                editFld.CUST_NAME = fld.CUST_NAME;
                editFld.COMPANY = fld.COMPANY;
                editFld.NPWP = fld.NPWP;
                editFld.address = fld.address;
                editFld.city = fld.city;
                editFld.province = fld.province;
                editFld.postal = fld.postal;
                editFld.Email = fld.Email;
                editFld.BANK_NAME = fld.BANK_NAME;
                editFld.BANK_NUMBER = fld.BANK_NUMBER;
                editFld.BANK_BRANCH = fld.BANK_BRANCH;
                editFld.BANK_COUNTRY = fld.BANK_COUNTRY;
                editFld.REG_DATE = fld.REG_DATE;
                editFld.Email = fld.Email;
                editFld.KTP = fld.KTP;
                editFld.PHONE1 = fld.PHONE1;
                editFld.PHONE2 = fld.PHONE2;
                editFld.VA1 = fld.VA1;
                editFld.VA2 = fld.VA2;
                editFld.VA1NOTE = fld.VA1NOTE;
                editFld.VA2NOTE = fld.VA2NOTE;
                editFld.discount_customer = fld.discount_customer;

                //editFld.FLAG_AKTIF = "0";
                if (fld.isBlackList == true)
                {
                    editFld.BL_FLAG = "1";
                }
                else
                {
                    editFld.BL_FLAG = "0";

                }

                if (fld.fileKtp != null)
                {
                    editFld.FILE_KTP_NAME = fld.fileKtp.FileName;
                    using (var ms = new MemoryStream())
                    {
                        fld.fileKtp.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        editFld.FILE_KTP = fileBytes;
                        string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                    }
                }
                if (fld.fileAkta != null)
                {
                    editFld.FILE_AKTA_NAME = fld.fileAkta.FileName;
                    using (var ms = new MemoryStream())
                    {
                        fld.fileAkta.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        editFld.FILE_AKTA = fileBytes;
                        string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                    }
                }
                if (fld.fileRekening != null)
                {
                    editFld.FILE_REKENING_NAME = fld.fileRekening.FileName;
                    using (var ms = new MemoryStream())
                    {
                        fld.fileRekening.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        editFld.FILE_REKENING = fileBytes;
                        string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                    }
                }
                if (fld.fileNPWP != null)
                {
                    editFld.FILE_NPWP_NAME = fld.fileNPWP.FileName;
                    using (var ms = new MemoryStream())
                    {
                        fld.fileNPWP.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        editFld.FILE_NPWP = fileBytes;
                        string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                    }
                }
                if (fld.fileTdp != null)
                {
                    editFld.FILE_TDP_NAME = fld.fileTdp.FileName;
                    using (var ms = new MemoryStream())
                    {
                        fld.fileTdp.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        editFld.FILE_TDP = fileBytes;
                        string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                    }
                }
                if (fld.fileSIUP != null)
                {
                    editFld.FILE_SIUP_NAME = fld.fileSIUP.FileName;
                    using (var ms = new MemoryStream())
                    {
                        fld.fileSIUP.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        editFld.FILE_SIUP = fileBytes;
                        string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                    }
                }
                if (fld.fileNIB != null)
                {
                    editFld.FILE_NIB_NAME = fld.fileNIB.FileName;
                    using (var ms = new MemoryStream())
                    {
                        fld.fileNIB.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        editFld.FILE_NIB = fileBytes;
                        string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                    }
                }
                if (fld.fileSPPKP != null)
                {
                    editFld.FILE_SPPKP_NAME = fld.fileSPPKP.FileName;
                    using (var ms = new MemoryStream())
                    {
                        fld.fileSPPKP.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        editFld.FILE_SPPKP = fileBytes;
                        string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                    }
                }
                if (fld.fileSKT != null)
                {
                    editFld.FILE_SKT_NAME = fld.fileSKT.FileName;
                    using (var ms = new MemoryStream())
                    {
                        fld.fileSKT.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        editFld.FILE_SKT = fileBytes;
                        string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                    }
                }
                if (fld.isApproveBool == true)
                {
                    editFld.isApproved = "1";
                }
                else
                {
                    editFld.isApproved = "0";
                }
                if (fld.isApproveBool2 == true)
                {
                    if (editFld.isApproved2 != "1")
                    {
                        editFld.isApproved2 = "1";

                        string mySqlConnectionStr = Configuration.GetConnectionString("DefaultConnection");

                        using (MySqlConnection conn = new MySqlConnection(mySqlConnectionStr))
                        {
                            conn.Open();
                            string query = @"update aspnetusers set EmailConfirmed = '1' where Email  = '" + editFld.Email + "'";

                            MySqlCommand cmd = new MySqlCommand(query, conn);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    //pricedt.articleprice = Convert.ToInt32(reader["price"]);

                                }
                            }
                            conn.Close();
                        }
                        var getuser = _userManager.FindByEmailAsync(editFld.Email.Trim());
                        IdentityUser userdata = getuser.Result;
                        addrole(userdata);
                        SendWelcomeMail(editFld.Email.Trim());
                    }
                }
                else
                {
                    editFld.isApproved2 = "0";

                }
               
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

        [Authorize]
        public IActionResult Delete(int id)
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
            return View(fld);
        }
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCustomer(int id)
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
