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
    public class SliderUploadController : Controller
    {
        private readonly FormDBContext db;
        private IHostingEnvironment Environment;
        private readonly ILogger<SliderUploadController> _logger;

        public SliderUploadController(FormDBContext db, ILogger<SliderUploadController> logger, IHostingEnvironment _environment)
        {
            logger = logger;
            Environment = _environment;
            this.db = db;
        }
        [Authorize(Roles = "ContentAdmin")]
        public IActionResult Index()
        {
            var data = new List<dbSliderImg>();
            data = db.SlideTbl.Where(y => y.FLAG_AKTIF == "1").OrderBy(y => y.IMG_DESC).ToList();
            return View(data);
        }
        [HttpGet]
        [Authorize(Roles = "ContentAdmin")]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind] dbSliderImg obj)
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
                    if(obj.fileImg != null)
                    {
                        obj.FILE_NAME = obj.fileImg.FileName;
                        using (var ms = new MemoryStream())
                        {
                            obj.fileImg.CopyTo(ms);
                            var fileBytes = ms.ToArray();
                            obj.SLIDE_IMG_BLOB = fileBytes;
                            string s = Convert.ToBase64String(fileBytes);
                            // act on the Base64 data
                        }
                    }
                    db.SlideTbl.Add(obj);
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
        [Authorize(Roles = "ContentAdmin")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            dbSliderImg fld = db.SlideTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            else
            {
                if (fld.SLIDE_IMG_BLOB != null)
                {
                    string imageBase64Data = Convert.ToBase64String(fld.SLIDE_IMG_BLOB);
                    string imageDataURL = string.Format("data:image/png;base64,{0}", imageBase64Data);
                    ViewBag.ImageData = imageDataURL;
                }
            }
            return View(fld);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(int? id, [Bind] dbSliderImg fld)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                var editFld = db.SlideTbl.Find(id);
                editFld.IMG_DESC = fld.IMG_DESC;
                if (fld.fileImg != null)
                {
                    editFld.FILE_NAME = fld.fileImg.FileName;
                    using (var ms = new MemoryStream())
                    {
                        fld.fileImg.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        editFld.SLIDE_IMG_BLOB = fileBytes;
                        string s = Convert.ToBase64String(fileBytes);
                        // act on the Base64 data
                    }
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
       
       
        [Authorize(Roles = "ContentAdmin")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            dbSliderImg fld = db.SlideTbl.Find(id);
            if (fld == null)
            {
                return NotFound();
            }
            return View(fld);
        }
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteImg(int? id)
        {
            dbSliderImg fld = db.SlideTbl.Find(id);
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
