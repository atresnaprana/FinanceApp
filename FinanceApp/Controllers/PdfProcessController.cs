using Microsoft.AspNetCore.Mvc;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Interactive;
using System.IO;

namespace FinanceApp.Controllers
{
    public class PdfProcessController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("fill-form1770")]
        public IActionResult FillForm()
        {
            string templatePath = "wwwroot/templates/form1770.pdf"; // Your uploaded template
            string outputPath = "wwwroot/output/form1770_filled.pdf";

            // Load the existing PDF document
            using (FileStream inputPdf = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            using (PdfLoadedDocument loadedDoc = new PdfLoadedDocument(inputPdf))
            {
                // Access the form fields
                PdfLoadedForm form = loadedDoc.Form;

                // Fill fields (use the actual field names from the form!)
                (form.Fields["Npwp"] as PdfLoadedTextBoxField).Text = "123456789012345";
                (form.Fields["NamaWP"] as PdfLoadedTextBoxField).Text = "ADITYA TRESNAPRANA";
                (form.Fields["TahunPajak"] as PdfLoadedTextBoxField).Text = "25";
                (form.Fields["PTKP"] as PdfLoadedComboBoxField).SelectedIndex = 2;

                // Optional: flatten the form to make it uneditable
                //form.Flatten = true;

                // Save to output
                using (FileStream outputPdf = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    loadedDoc.Save(outputPdf);
                }
            }

            // Return as file download
            byte[] fileBytes = System.IO.File.ReadAllBytes(outputPath);
            return File(fileBytes, "application/pdf", "form1770_filled.pdf");
        }
    }
}
