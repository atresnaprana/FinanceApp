using BaseLineProject.Data;
using FinanceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Tesseract;

namespace FinanceApp.Controllers
{
    public class OcrController : Controller
    {
        private readonly string _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
        private readonly FormDBContext db;
        public const string SessionKeyNameFrom = "TransDateStrJpbFrom";
        public const string SessionKeyNameTo = "TransDateStrJpbTo";

        private IHostingEnvironment Environment;
        private readonly ILogger<JpbController> _logger;

        public OcrController(FormDBContext db, ILogger<OcrController> logger, IHostingEnvironment _environment)
        {
            logger = logger;
            Environment = _environment;
            this.db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Authorize]
        [HttpPost("submitinv")]
        public IActionResult SubmitInvoice([FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("No file uploaded.");
            try {
                string extractedText;

                using (var ms = new MemoryStream())
                {
                    imageFile.CopyTo(ms);
                    ms.Position = 0;

                    // Load Tesseract with Indonesian + English language
                    using (var engine = new TesseractEngine(_tessDataPath, "ind+eng", EngineMode.Default))
                    {
                        using (var img = Pix.LoadFromMemory(ms.ToArray()))
                        {
                            using (var page = engine.Process(img))
                            {
                                extractedText = page.GetText();
                            }
                        }
                    }
                }

                // Parse OCR result into structured data
                var parsedInvoice = InvoiceParser.ParseInvoice(extractedText);
                //List<dbJpb> jpbtbl = new List<dbJpb>();
                //List<dbJpn> jpntbl = new List<dbJpn>();
                //var invoiceno = parsedInvoice.InvoiceNo;
                //var date = parsedInvoice.Tanggal;
                //var convertdate = DateTime.ParseExact(parsedInvoice.Tanggal, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                //foreach (var datas in parsedInvoice.Details)
                //{
                //    var fld = datas;
                //    var combinedinfo = "Faktur: " + datas.FakturNo+ " | " + datas.Note1 + " | " + datas.Note2;
                //    var test = "";
                //    if (!datas.PurchaseVal.IsNullOrEmpty() && !datas.FakturDate.IsNullOrEmpty())
                //    {
                //        dbJpb addjpb = new dbJpb();
                //        addjpb.Akun_Debit = 5000001;
                //        addjpb.Akun_Credit = 1100002;
                //        addjpb.Description = combinedinfo;
                //        var transdate = DateTime.ParseExact(datas.FakturDate, "dd/MM/yy", System.Globalization.CultureInfo.InvariantCulture);
                //        addjpb.TransDate = transdate;
                //        addjpb.Value = Convert.ToInt32(datas.PurchaseVal.Replace(".", ""));
                //        var existingsales = db.JpbTbl.ToList();
                //        var number = "0001";
                //        var trans_nodata = existingsales.Select(y => new dbJpb()
                //        {
                //            Trans_no = y.Trans_no,
                //            shorttransno = y.Trans_no.Substring(0, 10),
                //            lasttransno = Convert.ToInt32(y.Trans_no.Substring(10, 4))


                //        }).ToList();

                //        var checkinvoicecurrent = trans_nodata.Where(y => y.shorttransno.Split("JPB_")[1] == transdate.ToString("ddMMyy")).ToList();

                //        if (checkinvoicecurrent.Count > 0)
                //        {
                //            var chkinvnum = checkinvoicecurrent.Max(y => y.lasttransno) + 1;
                //            if (chkinvnum.ToString().Length == 1)
                //            {
                //                number = "000" + chkinvnum.ToString();
                //            }
                //            else if (chkinvnum.ToString().Length == 2)
                //            {
                //                number = "00" + chkinvnum.ToString();
                //            }
                //            else if (chkinvnum.ToString().Length == 3)
                //            {
                //                number = "0" + chkinvnum.ToString();
                //            }
                //            else if (chkinvnum.ToString().Length == 4)
                //            {
                //                number = chkinvnum.ToString();
                //            }
                //        }
                //        addjpb.Trans_no = "JPB_" + transdate.ToString("ddMMyy") + number;

                //        addjpb.entry_date = DateTime.Now;
                //        addjpb.update_date = DateTime.Now;
                //        addjpb.entry_user = User.Identity.Name;
                //        addjpb.update_user = User.Identity.Name;
                //        addjpb.flag_aktif = "1";
                //        var datauser = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
                //        addjpb.company_id = datauser.COMPANY_ID;
                //        try
                //        {

                //            db.JpbTbl.Add(addjpb);
                //            db.SaveChanges();


                //        }
                //        catch (Exception ex)
                //        {
                //            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\ErrorLog");
                //            if (!Directory.Exists(filePath))
                //            {
                //                Directory.CreateDirectory(filePath);
                //            }
                //            using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath, "ErrMsgAdd" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".txt")))
                //            {
                //                outputFile.WriteLine(ex.ToString());
                //            }
                //        }
                //        //apprDal.AddApproval(objApproval);

                //    }
                //    else if (!datas.SaleVal.IsNullOrEmpty() && !datas.FakturDate.IsNullOrEmpty())
                //    {
                //        dbJpn addjpn = new dbJpn();
                //        addjpn.Akun_Debit = 1100002;
                //        addjpn.Akun_Credit = 4000001;
                //        addjpn.Description = combinedinfo;
                //        var transdate = DateTime.ParseExact(datas.FakturDate, "dd/MM/yy", System.Globalization.CultureInfo.InvariantCulture);
                //        addjpn.Value = Convert.ToInt32(datas.SaleVal.Replace(".", ""));

                //        addjpn.TransDate = transdate;

                //        var existingsales = db.JpnTbl.ToList();
                //        var number = "0001";
                //        var trans_nodata = existingsales.Select(y => new dbJpn()
                //        {
                //            Trans_no = y.Trans_no,
                //            shorttransno = y.Trans_no.Substring(0, 10),
                //            lasttransno = Convert.ToInt32(y.Trans_no.Substring(10, 4))


                //        }).ToList();

                //        var checkinvoicecurrent = trans_nodata.Where(y => y.shorttransno.Split("JPN_")[1] == transdate.ToString("ddMMyy")).ToList();

                //        if (checkinvoicecurrent.Count > 0)
                //        {
                //            var chkinvnum = checkinvoicecurrent.Max(y => y.lasttransno) + 1;
                //            if (chkinvnum.ToString().Length == 1)
                //            {
                //                number = "000" + chkinvnum.ToString();
                //            }
                //            else if (chkinvnum.ToString().Length == 2)
                //            {
                //                number = "00" + chkinvnum.ToString();
                //            }
                //            else if (chkinvnum.ToString().Length == 3)
                //            {
                //                number = "0" + chkinvnum.ToString();
                //            }
                //            else if (chkinvnum.ToString().Length == 4)
                //            {
                //                number = chkinvnum.ToString();
                //            }
                //        }
                //        addjpn.Trans_no = "JPN_" + transdate.ToString("ddMMyy") + number;

                //        addjpn.entry_date = DateTime.Now;
                //        addjpn.update_date = DateTime.Now;
                //        addjpn.entry_user = User.Identity.Name;
                //        addjpn.update_user = User.Identity.Name;
                //        addjpn.flag_aktif = "1";
                //        var datacustomer = db.CustomerTbl.Where(y => y.Email == User.Identity.Name).FirstOrDefault();
                //        addjpn.company_id = datacustomer.COMPANY_ID;
                //        try
                //        {

                //            db.JpnTbl.Add(addjpn);
                //            db.SaveChanges();


                //        }
                //        catch (Exception ex)
                //        {
                //            var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\ErrorLog");
                //            if (!Directory.Exists(filePath))
                //            {
                //                Directory.CreateDirectory(filePath);
                //            }
                //            using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath, "ErrMsgAdd" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".txt")))
                //            {
                //                outputFile.WriteLine(ex.ToString());
                //            }
                //        }
                //    }
                //}
                //return Ok("Data Has been Submitted");
                return Ok(parsedInvoice);

            }
            catch (Exception e)
            {
                return BadRequest("Error Exception: " + e.Message);

            }

        }
    }

    // ---------------- Data Models ----------------
    public class InvoiceHeader
    {
        public string Tanggal { get; set; }
        public string InvoiceNo { get; set; }
        public string PersonName { get; set; }
        public string Address { get; set; }
        public List<InvoiceDetail> Details { get; set; } = new List<InvoiceDetail>();
    }

    public class InvoiceDetail
    {
        public string FakturDate { get; set; }
        public string FakturNo { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
        public string PurchaseVal { get; set; }
        public string SaleVal { get; set; }
    }

    // ---------------- Parser ----------------
    public static class InvoiceParser
    {
        public static InvoiceHeader ParseInvoice(string ocrText)
        {
            var header = new InvoiceHeader();
            var lines = ocrText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(l => l.Trim())
                               .Where(l => !string.IsNullOrWhiteSpace(l))
                               .ToList();

            // --- ROBUST HEADER PARSING ---
            // Find the line containing the label, then extract the value.
            var tanggalLine = lines.FirstOrDefault(l => Regex.IsMatch(l, @"\b\d{2}[-./]\d{2}[-./]\d{4}\b"));
            if (tanggalLine != null) header.Tanggal = Regex.Match(tanggalLine, @"\b\d{2}[-./]\d{2}[-./]\d{4}\b").Value;

            var invoiceLine = lines.FirstOrDefault(l => l.ToLower().Contains("no. documen"));
            if (invoiceLine != null)
            {
                header.InvoiceNo = Regex.Match(invoiceLine, @"\b[\d\\]+").Value;
            }
            else
            { // Fallback for clean scan
                var longNumberLine = lines.FirstOrDefault(l => Regex.IsMatch(l, @"\b\d{10,}\b"));
                if (longNumberLine != null) header.InvoiceNo = longNumberLine;
            }

            var peternakLine = lines.FirstOrDefault(l => l.ToLower().Contains("peternak"));
            if (peternakLine != null) header.PersonName = Regex.Replace(peternakLine, @"(?i)peternak\s*[/,:\s]*plasma\s*[:,]?", "").Trim();

            var alamatLine = lines.FirstOrDefault(l => l.ToLower().Contains("alamat"));
            if (alamatLine != null) header.Address = Regex.Replace(alamatLine, @"(?i)alamat\s*:?", "").Trim();


            // --- LINE-CENTRIC DETAIL PARSING ---
            var detailLines = lines
                .SkipWhile(l => !l.ToLower().Contains("keterangan"))
                .Skip(1) // Skip the header row
                .TakeWhile(l => !l.ToLower().StartsWith("total"))
                .ToList();

            var dateRegex = new Regex(@"^\d{2}/\d{2}/\d{2,4}");
            var fakturRegex = new Regex(@"^\d{5,7}$");
            // Money regex now accepts numbers with dots OR long numbers without dots.
            var moneyRegex = new Regex(@"^(\d{1,3}(\.\d{3})*|\d{5,})$");

            foreach (var line in detailLines)
            {
                // Aggressively clean each line before processing
                var cleanedLine = Regex.Replace(line, @"[|\[\]_~]", " ");
                cleanedLine = Regex.Replace(cleanedLine, @"\s+", " ");

                var parts = cleanedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue; // Skip junk lines

                var detail = new InvoiceDetail();
                var descriptionParts = new List<string>();
                var moneyValues = new List<string>();

                foreach (var part in parts)
                {
                    var cleanPart = part.TrimEnd('.'); // Remove trailing dots from words like "tangkap."

                    if (detail.FakturDate == null && dateRegex.IsMatch(cleanPart))
                    {
                        detail.FakturDate = cleanPart;
                    }
                    else if (detail.FakturNo == null && fakturRegex.IsMatch(cleanPart))
                    {
                        detail.FakturNo = cleanPart;
                    }
                    else if (moneyRegex.IsMatch(cleanPart))
                    {
                        moneyValues.Add(CleanNumber(cleanPart));
                    }
                    else
                    {
                        descriptionParts.Add(cleanPart);
                    }
                }

                var fullDescription = string.Join(" ", descriptionParts);

                // Assign monetary values using heuristics
                if (moneyValues.Count == 1)
                {
                    if (IsSaleItem(fullDescription))
                        detail.SaleVal = moneyValues[0];
                    else
                        detail.PurchaseVal = moneyValues[0];
                }
                else if (moneyValues.Count >= 2) // Handle cases with two values
                {
                    detail.PurchaseVal = moneyValues[0];
                    detail.SaleVal = moneyValues[1];
                }

                var finalDescParts = fullDescription.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                detail.Note1 = finalDescParts.Length > 0 ? finalDescParts[0] : "";
                detail.Note2 = finalDescParts.Length > 1 ? finalDescParts[1] : "";

                header.Details.Add(detail);
            }

            return header;
        }

        private static bool IsSaleItem(string description)
        {
            var lowerDesc = description.ToLower();
            return lowerDesc.Contains("bebek") || lowerDesc.Contains("tangkap") || lowerDesc.Contains("tambahan");
        }

        private static string CleanNumber(string val)
        {
            var cleaned = Regex.Replace(val, @"[^\d]", "");
            if (long.TryParse(cleaned, out long num))
            {
                return num.ToString("N0", new CultureInfo("id-ID"));
            }
            return val; // Fallback
        }
    }
}
