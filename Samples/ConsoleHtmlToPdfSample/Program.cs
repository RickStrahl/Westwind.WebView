// Async or Callback
//#define UseAsync 

using Westwind.WebView.HtmlToPdf;
using Westwind.Utilities;
using System;



namespace ConsoleApp1
{        
    internal class Program
    {

#if UseAsync

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Generating Pdf file...");

            string outputFile = Path.Combine("c:\\temp", "test.pdf");
            File.Delete(outputFile);
            var pdfHost = new HtmlToPdfHost()
            {
                WebViewEnvironmentPath = "C:\\temp\\WebViewEnvironment"
            };

            // full file path or url
            var result = await pdfHost.PrintToPdfAsync(Path.GetFullPath("./HtmlSampleFileLonger-SelfContained.html"), outputFile);

            if (result.IsSuccess)
            {
                Console.WriteLine("Opening Pdf file (async): " + outputFile);
                ShellUtils.OpenUrl(outputFile);
            }else
            {
                Console.WriteLine("Pdf generation failed: " + result.Message);
            }
        }
#else
        // Use Events
        public static void Main(string[] args)
        {
            Console.WriteLine("Generating Pdf file...");

            string outputFile = Path.Combine("c:\\temp", "test.pdf");
            File.Delete(outputFile);

            // Using the non-extended version of the host (no TOC support)
            var pdfHost = new HtmlToPdfHost()
            {
                WebViewEnvironmentPath = "C:\\temp\\WebViewEnvironment"
            };
            var onPrintResult =  (PdfPrintResult result) => {
                if (result.IsSuccess)
                {
                    Console.WriteLine("Opening Pdf file (Callback): " + outputFile);
                    ShellUtils.OpenUrl(outputFile);
                }
                else
                {
                    Console.WriteLine("Pdf generation failed: " + result.Message);
                }

                Environment.Exit(0);
            };

            // full file path or url
            pdfHost.PrintToPdf(Path.GetFullPath("./HtmlSampleFileLonger-SelfContained.html"),  outputFile, onPrintResult);
           
            // wait for completion
            Console.ReadKey();
        }
#endif
    }
}