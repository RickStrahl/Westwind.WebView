using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;
using Westwind.WebView.HtmlToPdf;

namespace Westwind.WebView.Test.HtmlToPdf
{

    [TestClass]
    public class HtmlToPdfTests
    {
        /// <summary>
        /// Async Result operation - to file
        /// </summary>
        [TestMethod]
        public async Task PrintToPdfFileAsyncTest()
        {
            // File or URL to render
            //var url = "file:///C:/temp/TMPLOCAL/_MarkdownMonster_Preview.html";
            //var url = "C:\\temp\\TestReport.html";
            var url = Path.GetFullPath("./HtmlToPdf/HtmlSampleFileLonger-SelfContained.html");


            var htmlFile = url;
            var outputFile = Path.GetFullPath(@".\test2.pdf");

            File.Delete(outputFile);

            var host = new HtmlToPdfHost()
            {
                BackgroundHtmlColor = "#ffffff"
            };
            host.CssAndScriptOptions.KeepTextTogether = true;
            //host.CssAndScriptOptions.CssToInject = "h1 { color: red } h2 { color: green } h3 { color: goldenrod }";

            var pdfPrintSettings = new WebViewPrintSettings()
            {
                // margins are 0.4F default
                MarginTop = 0.5,
                MarginBottom = 0.3F,
                //ScaleFactor = 0.9F,

                ShouldPrintHeaderAndFooter = true,
                HeaderTitle = "Custom Header (centered)",
                FooterText = "Custom Footer (lower right)",

                // Optionally customize the header and footer completely - WebView syntax                
                // HeaderTemplate = "<div style='text-align:center; font-size: 12px;'><span class='title'></span></div>",
                // FooterTemplate = "<div style='text-align:right; margin-right: 2em'><span class='pageNumber'></span> of " +
                //                  "<span class='totalPages'></span></div>",

                GenerateDocumentOutline = true  // default
            };

            // output file is created
            var result = await host.PrintToPdfAsync(htmlFile, outputFile, pdfPrintSettings);

            Assert.IsTrue(result.IsSuccess, result.Message);
            ShellUtils.OpenUrl(outputFile);  // display it
        }



        /// <summary>
        /// Async Result Operation - to stream      
        /// </summary>        
        [TestMethod]
        public async Task PrintToPdfStreamAsyncTest()
        {
            var outputFile = Path.GetFullPath(@".\test3.pdf");
            var htmlFile = Path.GetFullPath("./HtmlToPdf/HtmlSampleFileLonger-SelfContained.html");

            var host = new HtmlToPdfHost()
            {
                BackgroundHtmlColor = "#ffffff"
            };
            host.CssAndScriptOptions.KeepTextTogether = true;

            var pdfPrintSettings = new WebViewPrintSettings()
            {
                // margins are 0.4F default
                MarginTop = 0.5,
                MarginBottom = 0.3F,
                ScaleFactor = 0.9F,   // 1 is default

                ShouldPrintHeaderAndFooter = true,
                HeaderTitle = "Custom Header (centered)",
                FooterText = "Custom Footer (lower right)",

                // Optionally customize the header and footer completely - WebView syntax                
                // HeaderTemplate = "<div style='text-align:center; font-size: 12px;'><span class='title'></span></div>",
                // FooterTemplate = "<div style='text-align:right; margin-right: 2em'><span class='pageNumber'></span> of " +
                //                  "<span class='totalPages'></span></div>",

                GenerateDocumentOutline = true  // default
            };

            // We're interested in result.ResultStream
            var result = await host.PrintToPdfStreamAsync(htmlFile, pdfPrintSettings);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.IsNotNull(result.ResultStream); // THIS

            // Copy resultstream to output file
            File.Delete(outputFile);
            using (var fstream = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.Write))
            {
                result.ResultStream.CopyTo(fstream);
                result.ResultStream.Close(); // Close returned stream!
            }
            ShellUtils.OpenUrl(outputFile);
        }

        /// <summary>
        /// Event callback on completion - to stream (in-memory)
        /// </summary>
        /// <remarks>
        /// Using async here only to facilitate waiting for completion.
        /// actual call does not require async calling method
        /// </remarks>
        [TestMethod]
        public async Task PrintToPdfStreamTest()
        {
            // File or URL
            var htmlFile = Path.GetFullPath("./HtmlToPdf/HtmlSampleFile-SelfContained.html");

            var tcs = new TaskCompletionSource<bool>();

            var host = new HtmlToPdfHost();
            Action<PdfPrintResult> onPrintComplete = (result) =>
            {
                if (result.IsSuccess)
                {
                    // create file so we can display
                    var outputFile = Path.GetFullPath(@".\test1.pdf");
                    File.Delete(outputFile);

                    using (var fstream = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        result.ResultStream.CopyTo(fstream);

                        result.ResultStream.Close(); // Close returned stream!                        
                        Assert.IsTrue(true);
                        ShellUtils.OpenUrl(outputFile);
                    }
                }
                else
                {
                    Assert.Fail(result.Message);
                }

                tcs.SetResult(true);
            };
            var pdfPrintSettings = new WebViewPrintSettings()
            {
                // default margins are 0.4F
                MarginBottom = 0.2F,
                MarginLeft = 0.2f,
                MarginRight = 0.2f,
                MarginTop = 0.4f,
                ScaleFactor = 0.8f,
                PageRanges = "1,2,5-8"
            };
            // doesn't wait for completion
            host.PrintToPdfStream(htmlFile, onPrintComplete, pdfPrintSettings);


            // wait for completion
            await tcs.Task;
        }

        /// <summary>
        /// Event callback on completion - to file
        /// </summary>
        /// <remarks>
        /// Using async here only to facilitate waiting for completion.
        /// actual call does not require async calling method
        /// </remarks>
        [TestMethod]
        public async Task PrintToPdfFileTest()
        {
            // File or URL
            var htmlFile = Path.GetFullPath("./HtmlToPdf/HtmlSampleFile-SelfContained.html");
            // Full Path to output file
            var outputFile = Path.GetFullPath(@".\test.pdf");
            File.Delete(outputFile);

            var tcs = new TaskCompletionSource<bool>();

            var host = new HtmlToPdfHost();

            Action<PdfPrintResult> onPrintComplete = (result) =>
            {
                if (result.IsSuccess)
                {
                    Assert.IsTrue(true);
                    ShellUtils.OpenUrl(outputFile);
                }
                else
                {
                    Assert.Fail(result.Message);
                }

                tcs.SetResult(true);
            };

            // doesn't wait for completion
            host.PrintToPdf(htmlFile, outputFile, onPrintComplete);

            // wait for completion
            await tcs.Task;
        }


        [TestMethod]
        public async Task InjectedCssTest()
        {
            var outputFile = Path.GetFullPath(@".\test3.pdf");
            var htmlFile = Path.GetFullPath("./HtmlToPdf/HtmlSampleFileLonger-SelfContained.html");

            var host = new HtmlToPdfHost();
            //host.CssAndScriptOptions.KeepTextTogether = true;
            host.CssAndScriptOptions.OptimizePdfFonts = true; // force built-in OS fonts (Segoe UI, apple-system, Helvetica) 
            host.CssAndScriptOptions.CssToInject = "h1 { color: red } h2 { color: green } h3 { color: goldenrod }";

            // We're interested in result.ResultStream
            var result = await host.PrintToPdfStreamAsync(htmlFile);

            Assert.IsTrue(result.IsSuccess, result.Message);
            Assert.IsNotNull(result.ResultStream); // THIS

            // Copy resultstream to output file
            File.Delete(outputFile);
            using (var fstream = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.Write))
            {
                result.ResultStream.CopyTo(fstream);
                result.ResultStream.Close(); // Close returned stream!
            }
            ShellUtils.OpenUrl(outputFile);
        }


        [TestMethod]
        public async Task PrintToPdfDarkMarginsFileAsyncTest()
        {
            // File or URL to render
            //var url = "file:///C:/temp/TMPLOCAL/_MarkdownMonster_Preview.html";
            //var url = "C:\\temp\\TestReport.html";
            var url = Path.GetFullPath("./HtmlToPdf/HtmlSampleFileLonger-SelfContained.html");


            var htmlFile = url;
            var outputFile = Path.GetFullPath(@".\test2.pdf");

            File.Delete(outputFile);

            var host = new HtmlToPdfHost()
            {
                BackgroundHtmlColor = "#111"
            };
            host.CssAndScriptOptions.KeepTextTogether = true;

            var pdfPrintSettings = new WebViewPrintSettings()
            {
                // margins are 0.4F default
                MarginTop = 0.5,
                MarginBottom = 0.5F,
                //ScaleFactor = 0.9F,

                // Custom Templates required for dark background so we can set text color
                ShouldPrintHeaderAndFooter = true,
                HeaderTemplate = "<div style='text-align:center; width: 100%; font-size: 12px; color: white'><span class='title'></span></div>",
                FooterTemplate = "<div style='text-align:right; width: 100%; font-size: 10px; color: white; margin-right: 2em;'><span class='pageNumber'></span> of " +
                                 "<span class='totalPages'></span></div>",


                GenerateDocumentOutline = true  // default
            };

            // output file is created
            var result = await host.PrintToPdfAsync(htmlFile, outputFile, pdfPrintSettings);

            Assert.IsTrue(result.IsSuccess, result.Message);
            ShellUtils.OpenUrl(outputFile);  // display it
        }


        [TestMethod]
        public void SettingsCultureJsonSerializationTests()
        {
            string expectedScale = "1.22";
            // Arrange
            var settings = new DevToolsPrintToPdfSettings
            {
                scale = 1.22,

            };
            CultureInfo.CurrentCulture = new CultureInfo("de-de");

            // Act
            var json = settings.ToJson();

            Console.WriteLine(json);

            // Assert
            Assert.IsTrue(json.Contains($"\"scale\":{expectedScale}"));
        }

    }
}