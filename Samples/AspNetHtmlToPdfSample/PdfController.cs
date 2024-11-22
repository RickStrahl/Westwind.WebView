using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Westwind.WebView.HtmlToPdf;

namespace WebApplication1
{

    /// <summary>
    /// IMPORTANT: This works when running locally using Kestrel on the desktop
    /// It **does not work** inside of a system context - ie. inside of IIS without a loging 
    /// (unless you run as INTERACTIVE)
    /// </summary>
    [ApiController]
    [Route("pdf")]
    public class PdfController : ControllerBase
    {
        /// <summary>
        /// Default result = return JSON object with embedded binary data
        /// </summary>
        /// <returns></returns>
        [HttpGet]
       public async Task<object> Get()
       {
            var file = Path.GetFullPath("./HtmlSampleFile-SelfContained.html");

            var pdf = new HtmlToPdfHost();
            var pdfResult = await pdf.PrintToPdfStreamAsync(file, new WebViewPrintSettings {  PageRanges = "1-5"});

            if (pdfResult == null || !pdfResult.IsSuccess) 
            {
                return new
                {
                    IsError = true,
                    Message = pdfResult.Message
                };                
            }
            Response.StatusCode = 200;

            return new
            {
                IsError = false,
                PdfBytes = (pdfResult.ResultStream as MemoryStream).ToArray()
            };
       }

        /// <summary>
        /// Return raw data as PDF
        /// </summary>
        /// <returns></returns>
        [HttpGet("rawpdf")]
        public async Task<IActionResult> RawPdf()
        {
            var file = Path.GetFullPath("./HtmlSampleFile-SelfContained.html");

            var pdf = new HtmlToPdfHost();
            var pdfResult = await pdf.PrintToPdfStreamAsync(file, new WebViewPrintSettings {  PageRanges = "1-10"});

            if (pdfResult == null || !pdfResult.IsSuccess)
            {
                Response.StatusCode = 500;                
                return new JsonResult(new
                {
                    isError = true,
                    message = pdfResult.Message
                });
            }

            return new FileStreamResult(pdfResult.ResultStream, "application/pdf");             
        }

        /// <summary>
        /// Return raw data as PDF
        /// </summary>
        /// <returns></returns>
        [HttpGet("PdfFromUrl")]
        public async Task<IActionResult> PdfFromUrl([FromQuery] string url)
        {            
            if (string.IsNullOrEmpty(url))
                url = Path.GetFullPath("./HtmlSampleFile-SelfContained.html");

            var pdf = new HtmlToPdfHost();
            var pdfResult = await pdf.PrintToPdfStreamAsync(url, new WebViewPrintSettings {  });

            if (pdfResult == null || !pdfResult.IsSuccess)
            {
                Response.StatusCode = 500;
                return new JsonResult(new
                {
                    isError = true,
                    message = pdfResult.Message
                });
            }

            return new FileStreamResult(pdfResult.ResultStream,  "application/pdf");
        }

        /// <summary>
        /// Status info to ensure app works
        /// </summary>
        /// <returns></returns>
        [HttpGet("ping")]
        public object Ping()
        {
            return new
            {
                Message = "Hello World.",
                Time = DateTime.Now,
                User = Environment.UserName,
                LoggedOnUser = User?.Identity?.Name
            };
        }
    }
}
