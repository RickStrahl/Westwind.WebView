using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Westwind.WebView.HtmlToPdf
{
    public class HtmlToPdfDefaults
    {
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
        public static string BackgroundHtmlColor { get; set; } = "#ffffff";

        /// <summary>
        /// If true uses WebView.PrintToPdfStreamAsync() rather than the DevTools version
        /// to generate PDF output. Use this for server operation as the DevTools printing 
        /// is not supported in server environments like IIS.
        /// </summary>
        public static bool UseClassicPdfGeneration { get; set; } = false;


        /// <summary>
        /// The default folder location for the WebView environment folder that is used when
        /// no explicit path is provided. This is a static value that is global to 
        /// the Application, so it's best set during application startup to ensure
        /// that the same folder is used.
        /// 
        /// Make sure this folder is writable by the application's user identity!
        /// </summary>
        public static string WebViewEnvironmentPath { get; set; } = Path.Combine(Path.GetTempPath(), "WebView2_Environment");
          


        /// <summary>
        /// Pre-initializes the Print Service which is necessary when running under
        /// server environment
        /// </summary>
        /// <param name="defaultWebViewEnvironmentPath">
        /// Provide a folder where the WebView Environment can be written to. 
        /// 
        /// *** IMPORTANT: ***
        /// This location has to be writeable using the server's identity so 
        /// be sure to set folder permissions for limited user accounts.
        /// 
        /// Defaults to the User Temp folder, but for server apps that folder may
        /// not be accessible, so it's best to explicitly set and configure this
        /// folder.
        /// </param>
        public static void ServerPreInitialize(string defaultWebViewEnvironmentPath = null)
        {
            if (!string.IsNullOrEmpty(defaultWebViewEnvironmentPath))
                HtmlToPdfDefaults.WebViewEnvironmentPath = defaultWebViewEnvironmentPath;

            UseClassicPdfGeneration = true;

            Task.Run(async () =>
            {
                try
                {
                    var host = new HtmlToPdfHost();
                    var result = await host.PrintToPdfStreamAsync("about:blank");
                }
                catch (Exception ex)
                {
                }
            });
        }
    }
}
