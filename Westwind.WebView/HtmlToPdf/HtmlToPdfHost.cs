using Microsoft.Web.WebView2.Core;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Westwind.WebView.HtmlToPdf
{


    /// <summary>
    /// Converts an HTML document to PDF using the Windows WebView control.    
    /// </summary>
    /// <remarks>
    /// * Recommend you use a new instance for each PDF generation
    /// * Works only on Windows
    /// * Requires net8.0-windows target to work
    /// </remarks>
    public class HtmlToPdfHost
    {        
        internal WebViewPrintSettings WebViewPrintSettings = new WebViewPrintSettings();
        internal TaskCompletionSource<bool> IsCompleteTaskCompletionSource { get; set; } = new TaskCompletionSource<bool>();

        /// <summary>
        /// A flag you can check to see if the conversion process has completed.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// The location of the WebView environment folder that is required
        /// for WebView operation. Uses a default in the temp folder but you
        /// can customize to use an application specific folder.
        /// 
        /// (If you already use a WebView keep all WebViews pointing at the same environment: 
        /// https://weblog.west-wind.com/posts/2023/Oct/31/Caching-your-WebView-Environment-to-manage-multiple-WebView2-Controls
        /// </summary>
        public string WebViewEnvironmentPath { get; set; } = Path.Combine(Path.GetTempPath(), "WebView2_Environment");


        /// <summary>
        /// Options to inject and optimize CSS for print operations in PDF generation.
        /// </summary>
        public PdfCssAndScriptOptions CssAndScriptOptions { get; set; } = new PdfCssAndScriptOptions();


        /// <summary>
        /// Specify the background color of the PDF frame which contains
        /// the margins of the document. 
        ///
        /// Defaults to white, but if you use a non-white background for your
        /// document you'll likely want to match it to your document background.
        /// 
        /// Also note that non-white colors may have to use custom HeaderTemplate and 
        /// FooterTemplate to set the foregraound color of the text to match the background.
        /// </summary>
        public string BackgroundHtmlColor { get; set; } = "#ffffff";


        /// <summary>
        /// If set delays PDF generation to allow the document to complete loading if 
        /// content is dynamically loaded. By default PDF generation fires off 
        /// DomContentLoaded which fires when all embedded resources have loaded,
        /// but in some cases when resources load very slow, or when resources are dynamically
        /// loaded you might need to delay the PDF generation to allow the document to
        /// completely load.
        /// 
        /// Specify in milliseconds, default is 0 / no delay.
        /// </summary>
        public int DelayPdfGenerationMs { get; set; }


        /// <summary>
        /// This method prints a PDF from an HTML URl or File to PDF and awaits
        /// the result to be returned. Result is returned as a Memory Stream in
        /// result.ResultStream on success. 
        /// 
        /// Check result.IsSuccess to check for successful completion.
        /// </summary>
        /// <param name="url">File or URL to print to PDF</param>        
        /// <param name="webViewPrintSettings">WebView PDF generation settings</param>       
        public virtual Task<PdfPrintResult> PrintToPdfStreamAsync(string url,
            WebViewPrintSettings webViewPrintSettings = null)
        {
            IsComplete = false;
            WebViewPrintSettings = webViewPrintSettings ?? WebViewPrintSettings;

            PdfPrintResult result = new PdfPrintResult()
            {
                IsSuccess = false,
                Message = "PDF generation didn't complete.",
            };

            var tcs = new TaskCompletionSource<PdfPrintResult>();

            Thread thread = new Thread( () =>
            {
                // Create a Windows Forms Synchronization Context we can execute
                // which works without a desktop!
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                if (SynchronizationContext.Current == null)
                { 
                    tcs.SetResult(new PdfPrintResult {  IsSuccess = false, Message = "Couldn't create STA Synchronization Context." });
                    return;
                }
                SynchronizationContext.Current.Post( async (state)=>                 
                {
                    try
                    {
                        IsComplete = false;
                        IsCompleteTaskCompletionSource = new TaskCompletionSource<bool>();

                        var host = new CoreWebViewHeadlessHost(this);
                        await host.PrintFromUrlStream(url);

                        await IsCompleteTaskCompletionSource.Task;  
              
                        if (!host.IsComplete)
                        {
                            result = new PdfPrintResult()
                            {
                                IsSuccess = false,
                                Message = "Pdf generation timed out or failed to render inside of a non-Desktop context."
                            };
                        }
                        else
                        {
                            result = new PdfPrintResult()
                            {
                                IsSuccess = host.IsSuccess,
                                Message = host.IsSuccess ? "PDF was generated." : "PDF generation failed: " + host.LastException?.Message,
                                ResultStream = host.ResultStream,
                                LastException = host.LastException
                            };
                        }                        
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        result.IsSuccess = false;
                        result.Message = ex.ToString();
                        result.LastException = ex;
                        tcs.SetResult(result);
                    }
                    finally
                    {
                        IsComplete = true;
                        Application.ExitThread();  // now kill the event loop and thread
                    }
                }, null);                
                Application.Run();  // Windows Event loop needed for WebView in system context!
            });

            thread.SetApartmentState(ApartmentState.STA); // MUST BE STA!
            thread.Start();

            return tcs.Task;
        }

        /// <summary>
        /// This method prints a PDF from an HTML URl or File to PDF and awaits
        /// the result to be returned. Result is returned as a Memory Stream in
        /// result.ResultStream on success. 
        /// 
        /// Check result.IsSuccess to check for successful completion.
        /// </summary>        
        /// <param name="htmlStream">Stream of an HTML document to print to PDF</param>
        /// <param name="webViewPrintSettings">WebView PDF generation settings</param>       
        /// <param name="encoding">Encoding of the HTML stream. Defaults to UTF-8</param>
        public virtual Task<PdfPrintResult> PrintToPdfStreamAsync(Stream htmlStream,
            WebViewPrintSettings webViewPrintSettings = null,
            Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            IsComplete = false;
            WebViewPrintSettings = webViewPrintSettings ?? WebViewPrintSettings;

            PdfPrintResult result = new PdfPrintResult()
            {
                IsSuccess = false,
                Message = "PDF generation didn't complete.",
            };

            var tcs = new TaskCompletionSource<PdfPrintResult>();

            Thread thread = new Thread(() =>
            {
                // Create a Windows Forms Synchronization Context we can execute
                // which works without a desktop!
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                if (SynchronizationContext.Current == null)
                {
                    tcs.SetResult(new PdfPrintResult { IsSuccess = false, Message = "Couldn't create STA Synchronization Context." });
                    return;
                }
                SynchronizationContext.Current.Post(async (state) =>
                {
                    try
                    {
                        IsComplete = false;
                        IsCompleteTaskCompletionSource = new TaskCompletionSource<bool>();

                        var host = new CoreWebViewHeadlessHost(this);
                        await host.PrintFromHtmlStreamToStream(htmlStream, encoding);

                        await IsCompleteTaskCompletionSource.Task;

                        if (!host.IsComplete)
                        {
                            result = new PdfPrintResult()
                            {
                                IsSuccess = false,
                                Message = "Pdf generation timed out or failed to render inside of a non-Desktop context."
                            };
                        }
                        else
                        {
                            result = new PdfPrintResult()
                            {
                                IsSuccess = host.IsSuccess,
                                Message = host.IsSuccess ? "PDF was generated." : "PDF generation failed: " + host.LastException?.Message,
                                ResultStream = host.ResultStream,
                                LastException = host.LastException
                            };
                        }
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        result.IsSuccess = false;
                        result.Message = ex.ToString();
                        result.LastException = ex;
                        tcs.SetResult(result);
                    }
                    finally
                    {
                        IsComplete = true;
                        Application.ExitThread();  // now kill the event loop and thread
                    }
                }, null);
                Application.Run();  // Windows Event loop needed for WebView in system context!
            });

            thread.SetApartmentState(ApartmentState.STA); // MUST BE STA!
            thread.Start();

            return tcs.Task;
        }


    
        // await WebBrowser.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.printToPdf", "{}");




        /// <summary>
        /// This method prints a PDF from an HTML URl or File to PDF and awaits
        /// the result to be returned. Check result.IsSuccess to check for 
        /// successful completion of the file output generation or use File.Exists()
        /// </summary>
        /// <param name="url">File or URL to print to PDF</param>
        /// <param name="outputFile">output file for generated PDF</param>
        /// <param name="webViewPrintSettings">WebView PDF generation settings</param>
        public virtual Task<PdfPrintResult> PrintToPdfAsync(string url, 
            string outputFile,            
            WebViewPrintSettings webViewPrintSettings = null)
        {
            IsComplete = false;
            WebViewPrintSettings = webViewPrintSettings ?? WebViewPrintSettings;

            PdfPrintResult result = new PdfPrintResult { 
                IsSuccess = false,
                Message = "PDF generation didn't complete.",
            };

            var tcs = new TaskCompletionSource<PdfPrintResult>();
            Thread thread = new Thread(() =>
            {
                // Create a Windows Forms Synchronization Context we can execute
                // which works without a desktop!
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                if (SynchronizationContext.Current == null)
                {
                    tcs.SetResult(new PdfPrintResult { IsSuccess = false, Message = "Couldn't create STA Synchronization Context." });
                    return;
                }
                SynchronizationContext.Current.Post(async (state) =>
                {
                    try
                    {
                        IsComplete = false;
                        IsCompleteTaskCompletionSource = new TaskCompletionSource<bool>();

                        var host = new CoreWebViewHeadlessHost(this);
                        await host.PrintFromUrl(url, outputFile);

                        await IsCompleteTaskCompletionSource.Task;

                        if (!host.IsComplete)
                        {
                            result = new PdfPrintResult()
                            {
                                IsSuccess = false,
                                Message = "Pdf generation timed out or failed to render inside of a non-Desktop context."
                            };
                        }
                        else
                        {
                            result = new PdfPrintResult()
                            {
                                IsSuccess = host.IsSuccess,
                                Message = host.IsSuccess ? "PDF was generated." : "PDF generation failed: " + host.LastException?.Message,                                
                                LastException = host.LastException
                            };
                        }                        
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        result.IsSuccess = false;
                        result.Message = ex.ToString();
                        result.LastException = ex;
                        tcs.SetResult(result);
                    }
                    finally
                    {
                        IsComplete = true;
                        Application.ExitThread();  // now kill the event loop and thread
                    }
                }, null);
                Application.Run();  // Windows Event loop needed for WebView in system context!
            });

            thread.SetApartmentState(ApartmentState.STA); // MUST BE STA!
            thread.Start();

            return tcs.Task;            
        }


        /// <summary>
        /// This method prints a PDF from an HTML URl or File to PDF 
        /// using a new thread and a hosted form returning the result
        /// as an in-memory stream in result.ResultStream.
        /// 
        /// You get notified via OnPrintCompleteAction 'event' (Action) when the 
        /// output operation is complete.
        /// </summary>
        /// <param name="url">The filename or URL to print to PDF</param>
        /// <param name="onPrintComplete">Optional action to fire when printing (or failure) is complete</param>
        /// <param name="webViewPrintSettings">PDF output options</param>
        public virtual void PrintToPdfStream(string url, Action<PdfPrintResult> onPrintComplete = null, WebViewPrintSettings webViewPrintSettings = null)
        {
            WebViewPrintSettings = webViewPrintSettings ?? WebViewPrintSettings;

            PdfPrintResult result = new PdfPrintResult
            {
                IsSuccess = false,
                Message = "PDF generation didn't complete.",
            };

            Thread thread = new Thread(() =>
            {
                // Create a Windows Forms Synchronization Context we can execute
                // which works without a desktop!
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                if (SynchronizationContext.Current == null)
                {
                    IsComplete = true;
                    onPrintComplete?.Invoke(new PdfPrintResult { IsSuccess = false, Message = "Couldn't create STA Synchronization Context." });
                    return;
                }
                SynchronizationContext.Current.Post(async (state) =>
                {
                    try
                    {
                        IsComplete = false;
                        IsCompleteTaskCompletionSource = new TaskCompletionSource<bool>();

                        var host = new CoreWebViewHeadlessHost(this);
                        await host.PrintFromUrlStream(url);

                        await IsCompleteTaskCompletionSource.Task;

                        if (!host.IsComplete)
                        {
                            result = new PdfPrintResult()
                            {
                                IsSuccess = false,
                                Message = "Pdf generation timed out or failed to render inside of a non-Desktop context."
                            };
                        }
                        else
                        {
                            result = new PdfPrintResult()
                            {
                                IsSuccess = host.IsSuccess,
                                Message = host.IsSuccess ? "PDF was generated." : "PDF generation failed: " + host.LastException?.Message,
                                ResultStream = host.ResultStream,
                                LastException = host.LastException
                            };
                        }
                        onPrintComplete?.Invoke(result);                        
                    }
                    catch (Exception ex)
                    {
                        result.IsSuccess = false;
                        result.Message = ex.ToString();
                        result.LastException = ex;                        
                    }
                    finally
                    {
                        IsComplete = true;
                        Application.ExitThread();  // now kill the event loop and thread
                    }
                }, null);
                Application.Run();  // Windows Event loop needed for WebView in system context!
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// This method prints a PDF from an HTML Url or File to PDF 
        /// using a new thread and a hosted form. The method **returns immediately**
        /// and returns completion via the `onPrintComplete` Action parameter.
        /// 
        /// This method works in non-UI scenarios as it creates its own STA thread
        /// </summary>
        /// <param name="url">The filename or URL to print to PDF</param>
        /// <param name="outputFile">File to generate the output to</param>
        /// <param name="onPrintComplete">Action to fire when printing is complete</param>
        /// <param name="webViewPrintSettings">PDF output options</param>
        public virtual void PrintToPdf(string url, string outputFile,
            Action<PdfPrintResult> onPrintComplete = null,
            WebViewPrintSettings webViewPrintSettings = null)
        {
            WebViewPrintSettings = webViewPrintSettings ?? WebViewPrintSettings;

            PdfPrintResult result = new PdfPrintResult
            {
                IsSuccess = false,
                Message = "PDF generation didn't complete.",
            };

            Thread thread = new Thread(() =>
            {
                // Create a Windows Forms Synchronization Context we can execute
                // which works without a desktop!
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
                if (SynchronizationContext.Current == null)
                {
                    IsComplete = true;
                    onPrintComplete?.Invoke(new PdfPrintResult { IsSuccess = false, Message = "Couldn't create STA Synchronization Context." });
                    return;
                }
                SynchronizationContext.Current.Post(async (state) =>
                {
                    try
                    {
                        IsComplete = false;
                        IsCompleteTaskCompletionSource = new TaskCompletionSource<bool>();

                        var host = new CoreWebViewHeadlessHost(this);
                        await host.PrintFromUrl(url, outputFile);

                        await IsCompleteTaskCompletionSource.Task;

                        if (!host.IsComplete)
                        {
                            result = new PdfPrintResult()
                            {
                                IsSuccess = false,
                                Message = "Pdf generation timed out or failed to render inside of a non-Desktop context."
                            };
                        }
                        else
                        {
                            result = new PdfPrintResult()
                            {
                                IsSuccess = host.IsSuccess,
                                Message = host.IsSuccess ? "PDF was generated." : "PDF generation failed: " + host.LastException?.Message,
                                LastException = host.LastException
                            };
                        }
                        onPrintComplete?.Invoke(result);                        
                    }
                    catch (Exception ex)
                    {
                        result.IsSuccess = false;
                        result.Message = ex.ToString();
                        result.LastException = ex;                     
                    }
                    finally
                    {
                        IsComplete = true;
                        Application.ExitThread();  // now kill the event loop and thread
                    }
                }, null);
                Application.Run();  // Windows Event loop needed for WebView in system context!
            });
           
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();          
        }
    }

    public class  PdfCssAndScriptOptions 
    {
        /// <summary>
        /// Injects  @media print CSS that attempts to keep text from breaking across pages by:
        /// 
        /// * Minimizing paragraph breaks
        /// * List breaks
        /// * Keeping headers and following text together
        /// * Keeping code blocks from breaking
        /// 
        /// Uses page-break and break CSS styles to control page breaks. If you already have 
        /// @media print style in your HTML source you probably don't need this.
        /// </summary>
        public bool KeepTextTogether { get; set; } = false;

        /// <summary>
        /// Optionally inject custom CSS into the Html document header before printing.
        /// </summary>
        public string CssToInject { get; set; }


        /// <summary>
        /// If set to true adds fonts for Windows and Apple native fonts that work best
        /// for PDF generation using built-in fonts. This can help reduce the size of the
        /// PDF and also improve rendering for extended characters like emojis.
        /// 
        /// Use this if you see invalid characters in your PDF output
        /// </summary>
        public bool OptimizePdfFonts { get; set; }

        /// <summary>
        /// Not implemented yet.
        /// 
        /// Optionally inject custom JavaScript that can execute before the page is printed.
        /// Allows you to potentially modify the page before printing.
        /// </summary>
        public string ScriptToInject { get; set; }
    }

}
