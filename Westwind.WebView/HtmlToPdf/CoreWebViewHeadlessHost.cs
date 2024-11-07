using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Westwind.WebView.HtmlToPdf.Utilities;


namespace Westwind.WebView.HtmlToPdf
{

    /// <summary>
    /// This class provides the invisible WebView instance used to
    /// print the PDF. 
    /// </summary>
    internal class CoreWebViewHeadlessHost
    {
        /// <summary>
        /// The internal print settings picked up from the passed in host
        /// </summary>
        internal WebViewPrintSettings WebViewPrintSettings { get; set; } = new WebViewPrintSettings();

        private string _outputFile { get; set; }

        /// <summary>
        /// Passed in high level host
        /// </summary>
        internal HtmlToPdfHost HtmlToPdfHost { get; set; }

    

        internal bool IsSuccess { get; set; } = false;

        internal Exception LastException { get; set; }

        internal Stream ResultStream { get; set; }

        //internal Color Color { get; set; } = Color.White;

        /// <summary>
        /// Determines when PDF output generation is complete
        /// </summary>
        internal bool IsComplete { get; set; }

        /// <summary>
        /// The internal WebView instance we load and print from
        /// </summary>
        CoreWebView2 WebView { get; set; }

        private PdfPrintOutputModes PdfPrintOutputMode { get; set; } = PdfPrintOutputModes.File;

        protected TaskCompletionSource<bool> IsInitializedTaskCompletionSource = new TaskCompletionSource<bool>(); 

        internal CoreWebViewHeadlessHost(HtmlToPdfHost htmlToPdfHost)
        {
            HtmlToPdfHost = htmlToPdfHost;            
            WebViewPrintSettings = htmlToPdfHost.WebViewPrintSettings;
            InitializeAsync();
        }

        private IntPtr HWND_MESSAGE = new IntPtr(-3);

        protected async void InitializeAsync()
        {
            // must create a data folder if running out of a secured folder that can't write like Program Files
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: HtmlToPdfHost.WebViewEnvironmentPath);

            var controller = await environment.CreateCoreWebView2ControllerAsync(HWND_MESSAGE);
            controller.DefaultBackgroundColor = ColorTranslator.FromHtml(HtmlToPdfHost.BackgroundHtmlColor ?? "white");

            WebView = controller.CoreWebView2;                     
            WebView.DOMContentLoaded += CoreWebView2_DOMContentLoaded;

            // Ensure that control is initialized before we can navigate!
            IsInitializedTaskCompletionSource.SetResult(true);            
        }


        
        /// <summary>
        /// Internally navigates the the browser to the document to render
        /// </summary>
        /// <param name="url"></param>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        internal async Task PrintFromUrl(string url, string outputFile)
        {
            await IsInitializedTaskCompletionSource.Task;

            PdfPrintOutputMode = PdfPrintOutputModes.File;
            _outputFile = outputFile;
            WebView.Navigate(url);
        }

        /// <summary>
        /// Internally navigates t
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task PrintFromUrlStream(string url)
        {
            // Can't navigate until initialized
            await IsInitializedTaskCompletionSource.Task;

            PdfPrintOutputMode = PdfPrintOutputModes.Stream;
            WebView.Navigate(url);
        }

        /// <summary>
        /// Prints from an HTML stream. This allows HTML to be generated from
        /// in-memory sources
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public async Task PrintFromHtmlStreamToStream(Stream htmlStream,  Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            // Can't navigate until initialized
            await IsInitializedTaskCompletionSource.Task;

            WebView.Navigate("about:blank");

            PdfPrintOutputMode = PdfPrintOutputModes.Stream;
            htmlStream.Position = 0;
            string html = htmlStream.AsString(encoding);                                  


            string encodedHtml = html.ToJson();
            string script = "window.document.write(" + encodedHtml + ")";

            try
            {
                await WebView.ExecuteScriptAsync(script);
            }
            catch(Exception ex)
            {
                this.LastException = ex;
            }
        }



        private async void CoreWebView2_DOMContentLoaded(object sender, Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs e)
        {
            try
            {
                await InjectCssAndScript();
                
                if (PdfPrintOutputMode == PdfPrintOutputModes.File)
                    await PrintToPdf();               
                else
                    await PrintToPdfStream();
            }
            finally
            {
                IsComplete = true;
                HtmlToPdfHost.IsCompleteTaskCompletionSource.SetResult(true);
            }
        }

        private async Task InjectCssAndScript()
        {
            var css = new StringBuilder();

            if (HtmlToPdfHost.CssAndScriptOptions.OptimizePdfFonts)
            {
                css.AppendLine(OptimizedFontCss);
            }
            if (HtmlToPdfHost.CssAndScriptOptions.KeepTextTogether)
            {
                css.AppendLine(PageBreakCss);
            }
            if (!string.IsNullOrEmpty(HtmlToPdfHost.CssAndScriptOptions.CssToInject))
            {
                css.AppendLine(HtmlToPdfHost.CssAndScriptOptions.CssToInject);
            }
           

            if (css.Length > 0)
            {
                var script = "document.head.appendChild(document.createElement('style')).innerHTML = " + StringUtils.ToJson(css.ToString()) + ";";
                await WebView.ExecuteScriptAsync(script);
            }
        }



        /// <summary>
        /// Prints PDF to an output file
        /// </summary>
        /// <returns></returns>
        internal async Task PrintToPdf()
        {
            if (File.Exists(_outputFile))
                File.Delete(_outputFile);

            try
            {
                if (File.Exists(_outputFile))
                    File.Delete(_outputFile);

                // https://chromedevtools.github.io/devtools-protocol/tot/Page/#method-printToPDF
                //{
                //    "landscape": false,    
                //    "printBackground": true,
                //    "scale": 1,
                //    "paperWidth": 8.5,
                //    "paperHeight": 11,
                //    "marginTop": 0.50,
                //    "marginBottom": 0.30,
                //    "marginLeft": 0.40,
                //    "marginRight": 0.40,
                //    "pageRanges": "",  
                //    "headerTemplate": "<div style='font-size: 11.5px; width: 100%; text-align: center;'><span class='title'></span></div>",
                //    "footerTemplate": "<div style='font-size: 10px; clear: all; width: 100%; margin-right: 3em; text-align: right; '><span class='pageNumber'></span> of <span class='totalPages'></span></div>",
                //    "displayHeaderFooter": true,
                //    "preferCSSPageSize": false,
                //    "generateDocumentOutline": true
                //}

                if (HtmlToPdfHost.DelayPdfGenerationMs > 0)
                {
                    await Task.Delay(HtmlToPdfHost.DelayPdfGenerationMs);
                }

                var json = GetDevToolsWebViewPrintSettingsJson();       
                var pdfBase64 = await  WebView.CallDevToolsProtocolMethodAsync("Page.printToPDF", json);

                if (!string.IsNullOrEmpty(pdfBase64))
                {
                    // avoid JSON Serializer Dependency
                    var b64Data = StringUtils.ExtractString(pdfBase64,"\"data\":\"","\"}");
                    var pdfData = Convert.FromBase64String(b64Data);
                    File.WriteAllBytes(_outputFile, pdfData);    // 
                }

                //await WebView.PrintToPdfAsync(_outputFile, webViewPrintSettings);

                if (File.Exists(_outputFile))
                    IsSuccess = true;
                else
                    IsSuccess = false;
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                LastException = ex;
            }
        }

        


        /// <summary>
        /// Prints the current document in the WebView to a MemoryStream
        /// </summary>
        /// <returns></returns>
        internal async Task<Stream> PrintToPdfStream()
        {            
            try
            {
                var json = GetDevToolsWebViewPrintSettingsJson();
                Console.WriteLine(json);
                var pdfBase64 = await WebView.CallDevToolsProtocolMethodAsync("Page.printToPDF", json);

                if (!string.IsNullOrEmpty(pdfBase64))
                {
                    // avoid JSON Serializer Dependency
                    var b64Data = StringUtils.ExtractString(pdfBase64, "\"data\":\"", "\"}");
                    var pdfData = Convert.FromBase64String(b64Data);

                    var ms = new MemoryStream(pdfData);
                    ResultStream = ms;
                    IsSuccess = true;
                    return ResultStream;
                }

                IsSuccess = false;
                LastException = new InvalidOperationException("No PDF output was generated.");
                return null;
            }
            catch (Exception ex)
            {
                IsSuccess = false;
                LastException = ex;
                return null;
            }
        }

        /// <summary>
        /// Map our private type to the CoreWebView type.                
        /// </summary>
        /// <returns></returns>

        private CoreWebView2PrintSettings GetWebViewPrintSettings()
        {
            var wvps = WebView.Environment.CreatePrintSettings();

            var ps = WebViewPrintSettings;

            wvps.ScaleFactor = ps.ScaleFactor;
            wvps.MarginTop = ps.MarginTop;
            wvps.MarginBottom = ps.MarginBottom;
            wvps.MarginLeft = ps.MarginLeft;
            wvps.MarginRight = ps.MarginRight;

            wvps.PageWidth = ps.PageWidth;
            wvps.PageHeight = ps.PageHeight;

            wvps.Copies = ps.Copies;
            wvps.PageRanges = ps.PageRanges;

            wvps.ShouldPrintBackgrounds = ps.ShouldPrintBackgrounds;

            wvps.ShouldPrintHeaderAndFooter = ps.ShouldPrintHeaderAndFooter;
            wvps.HeaderTitle = ps.HeaderTemplate;
            wvps.FooterUri = ps.FooterTemplate;

            wvps.ShouldPrintSelectionOnly = ps.ShouldPrintSelectionOnly;
            wvps.Orientation = ps.Orientation == WebViewPrintOrientations.Portrait ? CoreWebView2PrintOrientation.Portrait : CoreWebView2PrintOrientation.Landscape;
            wvps.Duplex = ps.Duplex == WebViewPrintDuplexes.Default ? CoreWebView2PrintDuplex.Default :
                ps.Duplex == WebViewPrintDuplexes.OneSided ? CoreWebView2PrintDuplex.OneSided :
                ps.Duplex == WebViewPrintDuplexes.TwoSidedLongEdge ? CoreWebView2PrintDuplex.TwoSidedLongEdge :
                CoreWebView2PrintDuplex.TwoSidedShortEdge;
            wvps.Collation = ps.Collation == WebViewPrintCollations.Default ? CoreWebView2PrintCollation.Default :
                    ps.Collation == WebViewPrintCollations.Collated ? CoreWebView2PrintCollation.Collated :
                    CoreWebView2PrintCollation.Uncollated;
            wvps.ColorMode = ps.ColorMode == WebViewPrintColorModes.Color ? CoreWebView2PrintColorMode.Color : CoreWebView2PrintColorMode.Grayscale;


            wvps.PrinterName = ps.PrinterName;
            wvps.PagesPerSide = ps.PagesPerSide;

            return wvps;
        }


        /// <summary>
        /// Map WebViewPrintSettings to DevToolsPrintSettings and return as JSON
        /// that needs to be passed to the API.
        /// </summary>
        /// <returns></returns>
        public string GetDevToolsWebViewPrintSettingsJson() 
        {
            var wvps = new DevToolsPrintToPdfSettings();

            var ps = WebViewPrintSettings;

            wvps.landscape = ps.Orientation == WebViewPrintOrientations.Landscape;
            wvps.printBackground = ps.ShouldPrintBackgrounds;
            wvps.scale = ps.ScaleFactor;
            wvps.paperWidth = ps.PageWidth;
            wvps.paperHeight = ps.PageHeight;
            wvps.marginTop = ps.MarginTop;
            wvps.marginBottom = ps.MarginBottom;
            wvps.marginLeft = ps.MarginLeft;
            wvps.marginRight = ps.MarginRight;

            wvps.pageRanges = ps.PageRanges;

            wvps.displayHeaderFooter = ps.ShouldPrintHeaderAndFooter;
            wvps.headerTemplate = ps.HeaderTemplate;
            wvps.footerTemplate = ps.FooterTemplate;

            wvps.generateDocumentOutline = ps.GenerateDocumentOutline;

            return wvps.ToJson();
        }
        


        string PageBreakCss { get; } = @"
    html, body {
       text-rendering: optimizeLegibility;
       height: auto;
    }

    pre {
       white-space: pre-wrap;
       word-break: normal;
       word-wrap: normal;
    }
    pre > code {
        white-space: pre-wrap;
        padding: 1em !important;
    }

    /* keep paragraphs together */
    p, li, ul, code, pre {
       page-break-inside: avoid;
       break-inside: avoid;
    }

    /* keep headers and content together */
    h1, h2, h3, h4, h5, h6 {
       page-break-after: avoid;
       break-after: avoid;
    }
";
        string OptimizedFontCss { get; } = 
            @"html, body { font-family: ""Segoe UI Emoji"", ""Apple Color Emoji"", -apple-system, BlinkMacSystemFont,""Segoe UI"", Helvetica, Arial, sans-serif; }";   
        }
    }


public class DevToolsPrintToPdfSettings
{
    public bool landscape { get; set; } = false;
    
    public bool printBackground { get; set; } = true;
    
    public double scale { get; set; } = 1;
    public double paperWidth { get; set; } = 8.5;
    public double paperHeight { get; set; } = 11;
    public double marginTop { get; set; } = 0.4;
    public double marginBottom { get; set; } = 0.4;
    public double marginLeft { get; set; } = 0.4;
    public double marginRight { get; set; } = 0.4;
    public string pageRanges { get; set; } = "";

    public bool displayHeaderFooter { get; set; } = true;
    public string headerTemplate { get; set; } = "<div style='font-size: 10px; width: 100%; text-align: center;'><span class='title'></span></div>";
    public string footerTemplate { get; set; } = "<div style='font-size: 9px; width: 100%; text-align: right;'><span class='pageNumber'></span> of <span class='pageTotal'></span>";

    public bool preferCSSPageSize { get; set; } = false;
    public bool generateDocumentOutline { get; set; } = true;

    public string ToJson()
    {
        // avoid using a serializer
        return
$$"""
{      			
    "landscape": {{landscape.ToJson()}},    
    "printBackground": {{printBackground.ToJson()}},
    "scale": {{scale.ToJson()}},
    "paperWidth": {{paperWidth.ToJson()}},
    "paperHeight": {{paperHeight.ToJson()}},
    "marginTop": {{marginTop.ToJson()}},
    "marginBottom": {{marginBottom.ToJson()}},
    "marginLeft": {{marginLeft.ToJson()}},
    "marginRight": {{marginRight.ToJson()}},
    "pageRanges":  {{(pageRanges ?? string.Empty).ToJson()}},  
    "headerTemplate": {{headerTemplate.ToJson()}},
    "footerTemplate": {{footerTemplate.ToJson()}},
    "displayHeaderFooter": {{displayHeaderFooter.ToJson()}},
    "preferCSSPageSize": {{preferCSSPageSize.ToJson()}},
    "generateDocumentOutline": {{generateDocumentOutline.ToJson()}}                 
}			 
"""
                .Trim();
            

    }
}
        