using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Westwind.WebView.Wpf
{


    /// <summary>
    /// A wrapper class that provide a single point of initialization for the WebView2
    /// environment to ensure that only a single instance of the environment is created
    /// and used. This is important to avoid failures loading the environment with different
    /// environment settings is not actively supported by the WebView - IOW, it only
    /// supports a single environment per process and the first one determines the settings used,
    /// and unfortunately repeated instantiations with different settings can and often fail.
    /// 
    /// This cached environment ensures that only a single instance of the environment is created
    /// and used and provides a single point of initialization for the environment.
    /// </summary>
    public class CachedWebViewEnvironment
    {
        /// <summary>
        /// Cached instance used by default 
        /// </summary>
        public static CachedWebViewEnvironment Current { get; set; } = new CachedWebViewEnvironment();

        /// <summary>
        /// The Cached WebView Environment - one copy per running process
        /// </summary>
        public CoreWebView2Environment Environment { get; set; }

        /// <summary>
        /// The global setting for the folder where the WebView2 environment is stored.
        /// Defaults to the Temp folder of the current user. If not set. We recommend
        /// you set this folder **early** in your application startup.
        /// </summary>
        public string EnvironmentFolderName { get; set; }

        /// <summary>
        /// Optional WebView Environment options that can be set before the environment is created.
        /// Like the folder we recommend you set this early in your application startup.
        /// </summary>
        public CoreWebView2EnvironmentOptions EnvironmentOptions { get; set; }
        

        /// <summary>
        /// Ensure only one instance initializes the environment at a time to avoid
        /// multiple environment versions. Only applies to environment load, not waiting
        /// for the initialization to complete which can take a long time.
        /// </summary>
        private static SemaphoreSlim _EnvironmentLoadLock = new SemaphoreSlim(1, 1);


        /// <summary>
        /// This method provides a single point of initialization for the WebView2 environment
        /// to ensure that only a single instance of the environment is created. This is important
        /// to avoid failures loading the environment with different settings.
        /// 
        /// Once the environment is created it's cached in the Environment property and reused
        /// on subsequent calls. While possible to override the environment **we don't recommend it**.
        /// </summary>
        /// <remarks>
        /// This method does not complete until the WebView is UI activated. If not visible
        /// an `await` call will not complete until it becomes visible 
        /// (due to internal `WebView2.EnsureCoreWebView2Async()` behavior)
        /// </remarks>
        /// <param name="webBrowser">WebBrowser instance to set the environment on</param>
        /// <param name="environment">Optionally pass in an existing configured environment</param>
        /// <param name="webViewEnvironmentPath">Optional path to the WebView environment folder.</param>
        /// <param name="allowHostInputProcessing">If true, allows the host to process input events (ie. better transparency of browser keystrokes in host WPF app) .</param>
        /// <returns></returns>
        /// <exception cref="WebViewInitializationException"></exception>
        public async Task 
            InitializeWebViewEnvironment(WebView2 webBrowser, CoreWebView2Environment environment = null, 
                                                       string webViewEnvironmentPath = null, 
                                                       bool allowHostInputProcessing = false)
        {
            try
            {
                if (environment == null)
                    environment = Environment;

                if (environment == null)
                {
                    // lock
                    await _EnvironmentLoadLock.WaitAsync();

                    if (environment == null)
                    {
                        var envPath = webViewEnvironmentPath ?? Current.EnvironmentFolderName;
                        if (string.IsNullOrEmpty(envPath))
                                Current.EnvironmentFolderName = Path.Combine(Path.GetTempPath(),
                                Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location) + "_WebView");

                        // must create a data folder if running out of a secured folder that can't write like Program Files
                        environment = await CoreWebView2Environment.CreateAsync(userDataFolder: EnvironmentFolderName,
                            options: EnvironmentOptions);

                        Environment = environment;
                    }

                    _EnvironmentLoadLock.Release();
                }

                if (allowHostInputProcessing)
                {
                    var opts = Environment.CreateCoreWebView2ControllerOptions();
                    opts.AllowHostInputProcessing = true;                    
                    await webBrowser.EnsureCoreWebView2Async(environment, opts);
                }
                else
                    await webBrowser.EnsureCoreWebView2Async(environment);
            }
            catch (Exception ex)
            {
                throw new WebViewInitializationException($"WebView EnsureCoreWebView2AsyncCall failed.\nFolder: {EnvironmentFolderName}", ex);
            }
            
        }

    }
}
