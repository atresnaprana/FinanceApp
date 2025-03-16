using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;
using BaseLineProject.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using BaseLineProject.Data;
using BaseLineProject.Models;

namespace BaseLineProject.Areas.Identity
{
    [AllowAnonymous]
    public class RegisterCustomerModel : PageModel
    {
        private readonly FormDBContext db;

        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterCustomerModel> _logger;
        private readonly IMailService mailService;

        public RegisterCustomerModel(
           FormDBContext db,
           UserManager<IdentityUser> userManager,
           SignInManager<IdentityUser> signInManager,
           ILogger<RegisterCustomerModel> logger,
           IMailService mailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            this.db = db;
            this.mailService = mailService;

        }

        [BindProperty]
        public InputModel Input { get; set; }

        public dbCustomer datacust { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            //[Required]
            //[StringLength(100, ErrorMessage = "not valid KTP", MinimumLength = 16)]
            //[DataType(DataType.Text)]
            //[Display(Name = "KTP")]
            //public string KTP { get; set; }
            //[Required]
            //[DataType(DataType.Text)]
            //[Display(Name = "NPWP")]
            //public string NPWP { get; set; }
            [Required]
            [DataType(DataType.PhoneNumber)]
            [Display(Name = "Phone")]
            public string PHONE1 { get; set; }
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Nama")]
            public string CUST_NAME { get; set; }
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Company")]
            public string COMPANY { get; set; }
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Address")]
            public string Address { get; set; }
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "City")]
            public string City { get; set; }
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Province")]
            public string Province { get; set; }
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Postal")]
            public string Postal { get; set; }
            //[Required]
            //[DataType(DataType.Upload)]
            //[Display(Name = "KTP")]
            //public IFormFile fileKtp { get; set; }
            //[Required]
            //[DataType(DataType.Upload)]
            //[Display(Name = "Akta Perusahaan")]
            //public IFormFile fileAkta { get; set; }
            //[Required]
            //[DataType(DataType.Upload)]
            //[Display(Name = "Rekening Perusahaan")]
            //public IFormFile fileRekening { get; set; }
            //[Required]
            //[DataType(DataType.Upload)]
            //[Display(Name = "NPWP")]
            //public IFormFile fileNPWP { get; set; }
            //[Required]
            //[DataType(DataType.Upload)]
            //[Display(Name = "TDP")]
            //public IFormFile fileTdp { get; set; }
            //[Required]
            //[DataType(DataType.Upload)]
            //[Display(Name = "SIUP")]
            //public IFormFile fileSIUP { get; set; }
            //[Required]
            //[DataType(DataType.Upload)]
            //[Display(Name = "NIB")]
            //public IFormFile fileNIB { get; set; }
            //[Required]
            //[DataType(DataType.Upload)]
            //[Display(Name = "SPPKP / PKP")]
            //public IFormFile fileSPPKP { get; set; }
            //[Required]
            //[DataType(DataType.Upload)]
            //[Display(Name = "SKT")]
            //public IFormFile fileSKT { get; set; }
            //[Required]
            //[DataType(DataType.Text)]
            //[Display(Name = "Phone")]
            //public string store_area { get; set; }
            [NotMapped]
            public dbCustomer dataCust { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }
        [HttpPost]
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            try
            {
                if (ModelState.IsValid)
                {
                    var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                    var result = await _userManager.CreateAsync(user, Input.Password);
                    if (result.Succeeded)
                    {
                        var result1 = await _userManager.AddToRoleAsync(user, "CustomerOnPos");
                        var fieldCustomer = new dbCustomer();
                        fieldCustomer.CUST_NAME = Input.CUST_NAME;
                        fieldCustomer.Email = Input.Email;
                        fieldCustomer.PHONE1 = Input.PHONE1;
                        //fieldCustomer.KTP = Input.KTP;
                        //fieldCustomer.NPWP = Input.NPWP;
                        fieldCustomer.COMPANY = Input.COMPANY;
                        fieldCustomer.address = Input.Address;
                        fieldCustomer.city = Input.City;
                        fieldCustomer.province = Input.Province;
                        fieldCustomer.postal = Input.Postal;
                        //fieldCustomer.store_area = Input.store_area;
                        fieldCustomer.FLAG_AKTIF = "1";
                        fieldCustomer.REG_DATE = DateTime.Now;
                        fieldCustomer.UPDATE_DATE = DateTime.Now;
                        fieldCustomer.ENTRY_DATE = DateTime.Now;
                        fieldCustomer.ENTRY_USER = Input.Email;
                        fieldCustomer.UPDATE_USER = Input.Email;
                        fieldCustomer.BL_FLAG = "0";
                        //if (Input.fileKtp != null)
                        //{
                        //    fieldCustomer.FILE_KTP_NAME = Input.fileKtp.FileName;
                        //    using (var ms = new MemoryStream())
                        //    {
                        //        Input.fileKtp.CopyTo(ms);
                        //        var fileBytes = ms.ToArray();
                        //        fieldCustomer.FILE_KTP = fileBytes;
                        //        string s = Convert.ToBase64String(fileBytes);
                        //        // act on the Base64 data
                        //    }
                        //}
                        //if (Input.fileAkta != null)
                        //{
                        //    fieldCustomer.FILE_AKTA_NAME = Input.fileAkta.FileName;
                        //    using (var ms = new MemoryStream())
                        //    {
                        //        Input.fileAkta.CopyTo(ms);
                        //        var fileBytes = ms.ToArray();
                        //        fieldCustomer.FILE_AKTA = fileBytes;
                        //        string s = Convert.ToBase64String(fileBytes);
                        //        // act on the Base64 data
                        //    }
                        //}
                        //if (Input.fileRekening != null)
                        //{
                        //    fieldCustomer.FILE_REKENING_NAME = Input.fileRekening.FileName;
                        //    using (var ms = new MemoryStream())
                        //    {
                        //        Input.fileRekening.CopyTo(ms);
                        //        var fileBytes = ms.ToArray();
                        //        fieldCustomer.FILE_REKENING = fileBytes;
                        //        string s = Convert.ToBase64String(fileBytes);
                        //        // act on the Base64 data
                        //    }
                        //}
                        //if (Input.fileNPWP != null)
                        //{
                        //    fieldCustomer.FILE_NPWP_NAME = Input.fileNPWP.FileName;
                        //    using (var ms = new MemoryStream())
                        //    {
                        //        Input.fileNPWP.CopyTo(ms);
                        //        var fileBytes = ms.ToArray();
                        //        fieldCustomer.FILE_NPWP = fileBytes;
                        //        string s = Convert.ToBase64String(fileBytes);
                        //        // act on the Base64 data
                        //    }
                        //}
                        //if (Input.fileTdp != null)
                        //{
                        //    fieldCustomer.FILE_TDP_NAME = Input.fileTdp.FileName;
                        //    using (var ms = new MemoryStream())
                        //    {
                        //        Input.fileTdp.CopyTo(ms);
                        //        var fileBytes = ms.ToArray();
                        //        fieldCustomer.FILE_TDP = fileBytes;
                        //        string s = Convert.ToBase64String(fileBytes);
                        //        // act on the Base64 data
                        //    }
                        //}
                        //if (Input.fileSIUP != null)
                        //{
                        //    fieldCustomer.FILE_SIUP_NAME = Input.fileSIUP.FileName;
                        //    using (var ms = new MemoryStream())
                        //    {
                        //        Input.fileSIUP.CopyTo(ms);
                        //        var fileBytes = ms.ToArray();
                        //        fieldCustomer.FILE_SIUP = fileBytes;
                        //        string s = Convert.ToBase64String(fileBytes);
                        //        // act on the Base64 data
                        //    }
                        //}
                        //if (Input.fileNIB != null)
                        //{
                        //    fieldCustomer.FILE_NIB_NAME = Input.fileNIB.FileName;
                        //    using (var ms = new MemoryStream())
                        //    {
                        //        Input.fileNIB.CopyTo(ms);
                        //        var fileBytes = ms.ToArray();
                        //        fieldCustomer.FILE_NIB = fileBytes;
                        //        string s = Convert.ToBase64String(fileBytes);
                        //        // act on the Base64 data
                        //    }
                        //}
                        //if (Input.fileSPPKP != null)
                        //{
                        //    fieldCustomer.FILE_SPPKP_NAME = Input.fileSPPKP.FileName;
                        //    using (var ms = new MemoryStream())
                        //    {
                        //        Input.fileSPPKP.CopyTo(ms);
                        //        var fileBytes = ms.ToArray();
                        //        fieldCustomer.FILE_SPPKP = fileBytes;
                        //        string s = Convert.ToBase64String(fileBytes);
                        //        // act on the Base64 data
                        //    }
                        //}
                        //if (Input.fileSKT != null)
                        //{
                        //    fieldCustomer.FILE_SKT_NAME = Input.fileSKT.FileName;
                        //    using (var ms = new MemoryStream())
                        //    {
                        //        Input.fileSKT.CopyTo(ms);
                        //        var fileBytes = ms.ToArray();
                        //        fieldCustomer.FILE_SKT = fileBytes;
                        //        string s = Convert.ToBase64String(fileBytes);
                        //        // act on the Base64 data
                        //    }
                        //}

                        try
                        {
                            //db.Database.BeginTransaction();
                            db.CustomerTbl.Add(fieldCustomer);
                            db.SaveChanges();
                            //db.Dispose();
                        }
                        catch (Exception ex)
                        {
                            var testex = ex;
                        }

                        _logger.LogInformation("User created a new account with password.");

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                            protocol: Request.Scheme);

                        //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {

                            //var request = new MailRequest();
                            //request.Body = "body";
                            //request.Subject = "Test";
                            ////request.ToEmail ="aditya.tresnaprana@bata.com";
                            //request.ToEmail = Input.Email;
                            //try
                            //{
                            //    await mailService.SendEmailAsync(request);
                            //    //return Ok();
                            //}
                            //catch (Exception ex)
                            //{
                            //    throw;
                            //}
                            SendVerifyEmail(Input.Email, $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>");
                            //return RedirectToPage("RegisterComplete", new { email = Input.Email, returnUrl = returnUrl });
                            return RedirectToAction("Index", "RegisterComplete");

                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

            }
            catch (Exception ex)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\ErrorLog");
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath, "ErrMsgReg" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".txt")))
                {
                    outputFile.WriteLine(ex.ToString());
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
        public async void SendVerifyEmail(string Email, string callbackurl)
        {
            //string Email = "aditya.tresnaprana@bata.com";
            var request = new WelcomeRequest();
            request.UserName = Email;
            request.ToEmail = Email;
            request.callbackurl = callbackurl;
            //request.ToEmail = Input.Email;
            //var fileUrl = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot");
            //var files = Path.Combine(fileUrl, "FileCodeofConduct.pdf");
            //List<IFormFile> fileList = new List<IFormFile>();


            //using (var stream = System.IO.File.OpenRead(files))
            //{
            //    var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));


            //    fileList.Add(file);

            //    request.Attachments = fileList;
            //}
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
