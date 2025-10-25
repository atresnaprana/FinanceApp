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
        //[Authorize]
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
                //    var combinedinfo = datas.FakturNo + datas.Note1 + datas.Note2;
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
                //        //try
                //        //{

                //        //    db.JpbTbl.Add(addjpb);
                //        //    db.SaveChanges();


                //        //}
                //        //catch (Exception ex)
                //        //{
                //        //    var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\ErrorLog");
                //        //    if (!Directory.Exists(filePath))
                //        //    {
                //        //        Directory.CreateDirectory(filePath);
                //        //    }
                //        //    using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath, "ErrMsgAdd" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".txt")))
                //        //    {
                //        //        outputFile.WriteLine(ex.ToString());
                //        //    }
                //        //}
                //        //apprDal.AddApproval(objApproval);

                //    }
                //    if (!datas.SaleVal.IsNullOrEmpty() && !datas.FakturDate.IsNullOrEmpty())
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
                //        //try
                //        //{

                //        //    db.JpnTbl.Add(addjpn);
                //        //    db.SaveChanges();


                //        //}
                //        //catch (Exception ex)
                //        //{
                //        //    var filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\ErrorLog");
                //        //    if (!Directory.Exists(filePath))
                //        //    {
                //        //        Directory.CreateDirectory(filePath);
                //        //    }
                //        //    using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath, "ErrMsgAdd" + (DateTime.Now).ToString("dd-MM-yyyy HH-mm-ss") + ".txt")))
                //        //    {
                //        //        outputFile.WriteLine(ex.ToString());
                //        //    }
                //        //}
                //    }
                //}
                ////return Ok("Data Has been Submitted");
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

            var lines = ocrText.Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            // Extract tanggal (look for dd/MM/yy, dd/MM/yyyy, dd-MM-yyyy)
            var dateMatch = Regex.Match(ocrText, @"\b\d{1,2}[-/]\d{1,2}[-/]\d{2,4}\b");
            if (dateMatch.Success)
                header.Tanggal = dateMatch.Value;

            // Extract invoice number (after NO, NO., or similar OR at least 6+ digits in a row)
            var invMatch = Regex.Match(ocrText, @"(?:NO[:.\s]*)(?<inv>\d+)|\b\d{6,}\b", RegexOptions.IgnoreCase);
            if (invMatch.Success)
                header.InvoiceNo = invMatch.Groups["inv"].Success ? invMatch.Groups["inv"].Value : invMatch.Value;

            // --- FIX: Extract Person Name correctly ---
            // Look for the first line after InvoiceNo that is not "PERINCIAN", "PIUTANG", etc.
            header.PersonName = "";
            if (!string.IsNullOrEmpty(header.InvoiceNo))
            {
                int invIndex = lines.FindIndex(l => l.Contains(header.InvoiceNo));
                if (invIndex >= 0 && invIndex + 1 < lines.Count)
                {
                    for (int i = invIndex + 1; i < lines.Count; i++)
                    {
                        var candidate = lines[i];
                        if (Regex.IsMatch(candidate, @"PERINCIAN|PIUTANG|PLASMA|PERHITUNGAN", RegexOptions.IgnoreCase))
                            continue;

                        header.PersonName = candidate;
                        break;
                    }
                }
            }

            // Extract address
            header.Address = lines.FirstOrDefault(l => l.StartsWith("ALAMAT", System.StringComparison.OrdinalIgnoreCase)) ?? "";

            // Regex for rows: date | faktur | description | purchase | sale
            var rowRegex = new Regex(@"^(?<date>\d{2}/\d{2}/\d{2})\s*(?<faktur>\w+)?\s*(?<note>.+?)\s+(?<val1>[\d\.,]+)?\s*(?<val2>[\d\.,]+)?$");

            foreach (var line in lines)
            {
                var match = rowRegex.Match(line);
                if (match.Success)
                {
                    var notes = match.Groups["note"].Value.Split(' ', 2);
                    var note1 = notes.Length > 0 ? notes[0] : "";
                    var note2 = notes.Length > 1 ? notes[1] : "";

                    header.Details.Add(new InvoiceDetail
                    {
                        FakturDate = match.Groups["date"].Value,
                        FakturNo = match.Groups["faktur"].Value,
                        Note1 = note1,
                        Note2 = note2,
                        PurchaseVal = match.Groups["val1"].Value,
                        SaleVal = match.Groups["val2"].Value
                    });
                }
            }

            return header;
        }
    }


}
