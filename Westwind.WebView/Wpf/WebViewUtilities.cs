using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using Westwind.Utilities;

namespace Westwind.WebView.Wpf
{
    /// <summary>
    /// Installation and environment helpers for the WebView2
    /// control
    /// </summary>
    public class WebViewUtilities
    {

        /// <summary>
        /// This method checks to see if the WebView runtime is installed. It doesn't
        /// check for a specific version, just whether the runtime is installed at all.
        /// For a specific version check use IsWebViewVersionInstalled()
        /// 
        /// HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}
        /// HKEY_CURRENT_USER\Software\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5
        /// </summary>
        /// <returns></returns>
        public static bool IsWebViewRuntimeInstalled()
        {

            string regKey = @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients";
            using (RegistryKey edgeKey = Registry.LocalMachine.OpenSubKey(regKey))
            {
                if (edgeKey != null)
                {
                    string[] productKeys = edgeKey.GetSubKeyNames();
                    if (productKeys.Any())
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// This method checks to see if the WebView runtime is installed and whether it's
        /// equal or greater than the WebView .NET component.
        ///
        /// This ensures that you use a version of the WebView Runtime that supports the features
        /// of the .NET Component.
        /// 
        /// Should be called during app startup to ensure the WebView Runtime is available.
        /// </summary>
        public static bool IsWebViewVersionInstalled()
        {
            string versionNo = null;
            Version asmVersion = null;
            Version ver = null;

            try
            {
                versionNo = CoreWebView2Environment.GetAvailableBrowserVersionString();

                // strip off 'canary' or 'stable' version
                versionNo = StringUtils.ExtractString(versionNo, "", " ", allowMissingEndDelimiter: true)?.Trim();
                ver = new Version(versionNo);

                asmVersion = typeof(CoreWebView2Environment).Assembly.GetName().Version;

                if (ver.Build >= asmVersion.Build)
                    return true;
            }
            catch
            {
                // ignored
            }

            return false;
        }

        /// <summary>
        /// Removes the applications local WebView Environment
        /// </summary>
        /// <returns>
        /// true if the directory exists and was successfully deleted.
        /// false if directory doesn't exist, or the directory deletion fails.
        /// </returns>
        public static bool RemoveEnvironmentFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder))
                return false;

            var checkPath = Path.Combine(folder, "EbWebView");
            if (Directory.Exists(checkPath))
            {
                Directory.Delete(folder);
                if (!Directory.Exists(checkPath))
                    return true;
            }

            return false;
        }
    }
}
