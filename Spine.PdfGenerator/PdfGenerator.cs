using System;
using System.IO;
using System.Threading.Tasks;
using SelectPdf;
using Spine.RazorClassLibrary;

namespace Spine.PdfGenerator
{
    public interface IPdfGenerator
    {
        Task<byte[]> GeneratePdfByte<TModel>(string templateName, TModel model);
    }

    public class PdfGenerator : IPdfGenerator
    {
        private readonly IRazorViewToStringRenderer _razorRenderer;
        public PdfGenerator(IRazorViewToStringRenderer razorRenderer)
        {
            _razorRenderer = razorRenderer;
        }

        public async Task<byte[]> GeneratePdfByte<TModel>(string templateName, TModel model)
        {
            var genericPrintPath = "/Views/Prints/{0}.cshtml";
            var filePath = string.Format(genericPrintPath, templateName);

            var htmlContent = await _razorRenderer.RenderViewToStringAsync(filePath, model);

            // Create a PDF from an HTML Template--Select PDF
            var converter = new HtmlToPdf();

            // set converter rendering engine
            converter.Options.RenderingEngine = RenderingEngine.WebKit;

            converter.Options.PdfPageSize = PdfPageSize.A4;
            //Renderer.Options.MarginTop = 0;  //millimeters
            //Renderer.Options.MarginBottom = 0;
            //Renderer.Options.MarginLeft = 0;
            //Renderer.Options.MarginRight = 0;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            //  converter.Options.AutoFitHeight = HtmlToPdfPageFitMode.NoAdjustment;

            converter.Options.WebPageWidth = 1024;
            converter.Options.WebPageHeight = 0;
            converter.Options.WebPageFixedSize = false;

            converter.Options.AutoFitWidth = HtmlToPdfPageFitMode.ShrinkOnly;
            converter.Options.AutoFitHeight = HtmlToPdfPageFitMode.ShrinkOnly;

            // set css @media print
            converter.Options.CssMediaType = HtmlToPdfCssMediaType.Print;

            // set document passwords
            //converter.Options.SecurityOptions.OwnerPassword = "test1";
            //converter.Options.SecurityOptions.UserPassword = "test2";

            //set document permissions
            converter.Options.SecurityOptions.CanAssembleDocument = false;
            converter.Options.SecurityOptions.CanCopyContent = true;
            converter.Options.SecurityOptions.CanEditAnnotations = true;
            converter.Options.SecurityOptions.CanEditContent = true;
            converter.Options.SecurityOptions.CanFillFormFields = true;
            converter.Options.SecurityOptions.CanPrint = true;

            //Compression
            converter.Options.PdfCompressionLevel = PdfCompressionLevel.Best;
            converter.Options.JpegCompressionEnabled = true;
            converter.Options.JpegCompressionLevel = 70;

            byte[] filebyte;
            try
            {
                var PDF = converter.ConvertHtmlString(htmlContent); //won't work on non-Windows OS
                PDF.DocumentInformation.CreationDate = DateTime.Today;
                PDF.DocumentInformation.Title = templateName;
                PDF.DocumentInformation.Subject = templateName;
                PDF.DocumentInformation.Keywords = templateName;

                using (MemoryStream stream = new MemoryStream())
                {
                    PDF.Save(stream);
                    PDF.Close();
                    filebyte = stream.ToArray();
                }


                //needs license
                //var htmlToPdf = new NReco.PdfGenerator.HtmlToPdfConverter();
                //filebyte = htmlToPdf.GeneratePdf(htmlContent);
            }
            catch (Exception e)
            {
                filebyte = null;
            }

            return filebyte;
        }
    }
}
