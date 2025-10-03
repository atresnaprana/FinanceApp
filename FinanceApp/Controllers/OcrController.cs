using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Tesseract;

namespace FinanceApp.Controllers
{
    public class OcrController : Controller
    {
        private readonly string _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("extract")]
        public IActionResult ExtractText([FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("No file uploaded.");

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

            return Ok(parsedInvoice);
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
