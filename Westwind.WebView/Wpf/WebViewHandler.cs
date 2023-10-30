using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Westwind.WebView.Utilities;

namespace Westwind.WebView.Wpf
{

    /// <summary>
    /// Generic WebViewHandler implementation that uses the BaseJavaScriptInterop object.
    /// Use this version if you don't need to customize Interop
    /// </summary>
    public class WebViewHandler : WebViewHandler<BaseJavaScriptInterop>        
        
    {
        /// <summary>
        /// Ctor 
        /// </summary>
        /// <param name="webViewBrowser"></param>
        /// <param name="webViewEnvironmentFolder">Folder where temporary files are stored - null for default</param>
        /// <param name="hostObject">Object that can receive callbacks from JavaScript - can be null</param>
        public WebViewHandler(WebView2 webViewBrowser,
            string webViewEnvironmentFolder = null, 
            object dotnetCallbackObject = null)
            : base(webViewBrowser, webViewEnvironmentFolder,  dotnetCallbackObject)
        {
        }
    }


    /// <summary>
    /// This class is a wrapper around the WebView control to make it easier
    /// to use inside of a WPF application.
    ///
    /// To use it in your WPF app, create a property of the WebViewHandler type
    /// and initialize it in your Window CTOR after InitializeComponent passing
    /// in the WebBrowser control and 
    /// </summary>
    /// 
    /// 
    public class WebViewHandler<TJsInterop> : IDisposable
            where TJsInterop : BaseJavaScriptInterop
    {

        /// <summary>
        /// The full folder path name where WebView environment is stored. If
        /// not set a standard folder is created in the TEMP path.
        /// </summary>  
        public string WebViewEnvironmentFolder { get; set; } 


        /// <summary>
        /// Optionally you can pass in an already created WebView Environment.
        /// IMPORTANT: You must set this value in the CTOR call
        /// ie. `new WebViewEnvironment&lt;T&gt;(...) { WebViewEnvironment = env}`
        /// 
        /// If an environment is passed in it overrides the `WebViewEnvironmentFolder`.
        /// 
        /// This setting is useful as it allows you to share the same environment
        /// instance across multiple WebView instances.        
        /// </summary>
        public CoreWebView2Environment WebViewEnvironment { get; set; }

        /// <summary>
        /// Object that can be used to access JavaScript operations on the
        /// Preview window. Runs global functions in the document using CallMethod()
        /// </summary>
        public TJsInterop JsInterop { get; set; }

        /// <summary>
        /// An object that can be called from JavaScript code and passed
        /// to the Web Browser.
        ///
        /// This object **MUST BE SET DURING INITIALIZATION**
        /// Exposed in JavaScript as:
        /// window.chrome.webview.hostObjects.dotnet
        /// window.chrome.webview.hostObjects.sync.dotnet
        /// </summary>
        public object HostObject { get; set;  }

        /// <summary>
        /// The name of the host object passed to JavaScript for
        /// callbacks.
        /// Exposed in JavaScript as:
        ///  window.chrome.webview.hostObjects.dotnet
        ///  window.chrome.webview.hostObjects.sync.dotnet
        /// </summary>
        public string HostObjectName { get; set; } = "dotnet";

        
        /// <summary>
        /// The base name for JavaScript Interop operations.
        /// Defaults to `window` but can be any globally
        /// accessible object (ie. `window.textEditor`)
        /// </summary>
        public string ClientBaseObjectName { get; set; } = "window";


        /// <summary>
        /// If set to true a WebView crash will display an error message
        /// and exit the application. Otherwise the error is ignored or
        /// you can override the OnProcessFailed handler.
        /// </summary>
        public bool ExitApplicationOnWebViewCrash { get; set;  } = false;

        /// <summary>
        /// Optional folder that is mapped to HostWebHostNameForFolder. If this value
        /// is set the host mapping is made, otherwise no host mapping is done.
        /// </summary>
        /// <remarks>
        /// Note you can vary this folder based on your #DEBUG setting and for DEBUG
        /// use a fixed local folder, while using an application relative path that is
        /// deployed with your app. This allows you to edit your HTML resources without
        /// having to recompile your code.
        /// </remarks>
        /// 
        public string HostWebRootFolder { get; set;  }


        /// <summary>
        /// The domain URL that is mapped to the local folder if HostWebRootFolder is set.
        /// </summary>
        public string HostWebHostNameForFolder { get; set; } = "https://localweb.com";

        /// <summary>
        /// Url to navigate to. Assigned after Web View is initialized to allow for
        /// host folder domain name to be valid so you don't get an invalid navigation
        /// prior to assignment.
        /// </summary>
        public string InitialUrl { get; set;  }

        /// <summary>
        /// Determines if DevTools are loaded on startup
        /// </summary>
        public bool ShowDevTools { get; set; }

        /// <summary>
        /// The underlying WebBrowser instance. Exposed here so we can override
        /// properties and hook events if necessary.
        /// </summary>
        public WebView2 WebBrowser { get; set; }

        
        

        /// <summary>
        /// Shortcut to visibility setting
        /// </summary>
        public bool IsVisible
        {
            get { return WebBrowser.Visibility == Visibility.Visible; }
            set
            {
                if (!value)
                    WebBrowser.Visibility = Visibility.Hidden;
                else
                    WebBrowser.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Determines whether the WebBrowser is loaded and initialized
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Determines if the WebBrowser has been initialized
        /// and is ready to navigate or update internal document content.
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// Determines whether the WebBrowser has initialized and is ready
        /// to navigate or update internal document content.
        /// </summary>
        public Action InitializeComplete { get; set; }

     
        #region Initialization

        /// <summary>
        /// IMPORTANT! Please ensure that you call InitializeAsync() explicitly:
        /// wvh = new WebViewHandler&lt;BaseJavaScriptInterop&gt;(webView, null, null);
        /// _ = wvh.InitializeAsync();
        /// </summary>
        /// <param name="webViewBrowser">WebView2 Browser instance</param>=
        /// <param name="webViewEnvironmentFolder">optional environment folder on disk - can be null</param>
        /// <param name="hostObject">optional .NET callback object - can be null exposes as .dotnet in JavaScript or override HostObjectName</param>
        public WebViewHandler(WebView2 webViewBrowser, string webViewEnvironmentFolder, object hostObject)
        {
            WebViewEnvironmentFolder = webViewEnvironmentFolder;
            HostObject = hostObject;
            WebBrowser = webViewBrowser;

            // must be out of band to ensure all Properties are set and initialized
            // and to ensure we get a proper WPF Async context
            //
            // IMPORTANT: Don't use InvokeAsync() as it will cause
            //            WebView initialization conflicts if multiple
            //            WebView controls are used
            // _ = WebBrowser.Dispatcher.InvokeAsync( async () =>  InitializeAsync().FireAndForget);   // don't use!
            // WebBrowser.Dispatcher.Invoke(  ()=>  InitializeAsync().FireAndForget(), DispatcherPriority.Loaded );
            WebBrowser.Dispatcher.Invoke(() => InitializeAsync().FireAndForget(), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Initializes the control - you can override this but make sure
        /// you call base() to call original functionality
        /// </summary>
        /// <returns></returns>
        protected async Task InitializeAsync()
        {
            if (JsInterop == null)
                JsInterop = CreateJsInteropInstance();

            if (!IsInitialized)  // Ensure this doesn't run more than once
            {
                await CachedWebViewEnvironment.Current.InitializeWebViewEnvironment(WebBrowser);

                //if (string.IsNullOrEmpty(WebViewEnvironmentFolder))
                //{
                //    WebViewEnvironmentFolder = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location) + "_WebView");
                //}

                //try
                //{
                //    if (WebViewEnvironment == null)
                //    {
                //        // must create a data folder if running out of a secured folder that can't write like Program Files
                //        WebViewEnvironment = await CoreWebView2Environment.CreateAsync(
                //            userDataFolder: WebViewEnvironmentFolder
                //        );
                //    }
                //    await WebBrowser.EnsureCoreWebView2Async(WebViewEnvironment);
                //    IsInitialized = true;
                //} catch (Exception ex)
                //{
                //    throw new WebViewInitializationException($"WebView EnsureCoreWebView2AsyncCall failed.\nFolder: {WebViewEnvironmentFolder}", ex);
                //}

                if(InitializeComplete != null)
                    InitializeComplete();   
            }
            
            if (TrackZoomFactor)
            {
                CaptureZoomFactor();
            }

            if (ShowDevTools)
            {
                WebBrowser.CoreWebView2.OpenDevToolsWindow();
            }

            WebBrowser.CoreWebView2.DOMContentLoaded += OnDomContentLoaded;
            WebBrowser.CoreWebView2.FrameNavigationStarting += OnNavigationStarting;
            WebBrowser.CoreWebView2.FrameNavigationCompleted += OnNavigationCompleted;
            WebBrowser.CoreWebView2.ProcessFailed += OnWebViewProcessFailed;
            WebBrowser.KeyDown += OnWebBrowserKeyDown;
            
            // Register a .NET Callback object accessible from JavaScript
            if (HostObject != null)
            {
                WebBrowser.Dispatcher.Invoke(() =>
                {
                    WebBrowser.CoreWebView2.AddHostObjectToScript(HostObjectName, HostObject);
                });
            }

            // Map local folder to a virtual domain name
            if (!string.IsNullOrEmpty(HostWebRootFolder))
            {
                WebBrowser.CoreWebView2.SetVirtualHostNameToFolderMapping(HostWebHostNameForFolder, HostWebRootFolder, CoreWebView2HostResourceAccessKind.Allow);
            }

            if (!string.IsNullOrEmpty(InitialUrl))
            {
                WebBrowser.Source = new Uri(InitialUrl);
            }
        }

  

        /// <summary>
        /// Returns an instance of the JavaScript callback object reference if
        /// requested.
        ///
        /// IMPORTANT: Make sure the CTOR Signature is correct and corresponds
        ///            Base implementation (no optional parameters.
        /// </summary>
        /// <returns></returns>
        protected TJsInterop CreateJsInteropInstance()
        {            
            try
            {
                // make sure the type constructor supports 2 parameters
                return Activator.CreateInstance(typeof(TJsInterop), new object[] { WebBrowser, ClientBaseObjectName + "." }) as TJsInterop;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Event overrides

        /// <summary>
        /// Fired after a address or link navigation has completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            
        }


        /// <summary>
        /// Fired before address or link navigation occurs - allows you to intercept navigation to perform
        /// non URL based navigation for particular links. Useful for handling app specific links like
        /// app://dosomething/parm1 for example.
        ///
        /// You can also stop navigation (as you would when you intercept with local app logic)
        /// or change the URL using the event args.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">event args that allow modifying navigation behavior</param>
        protected virtual  void OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
        }

        protected virtual void OnWebBrowserKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //// Handle Alt-Key forward to form so menus work
            //if (e.Key == System.Windows.Input.Key.LeftAlt && Window != null)
            //{
            //    Window.Focus();
            //    Window.Dispatcher.InvokeAsync(() => System.Windows.Forms.SendKeys.SendWait("%"));
            //}
        }

        /// <summary>
        /// Fired when DOM content has completed loading. Allows you access the document saefly after
        /// it has initially loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDomContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            IsLoaded = true;
        }


        /// <summary>
        /// Override this method to handle WebView process failures.
        ///
        /// The default behavior 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnWebViewProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
        {
            if (ExitApplicationOnWebViewCrash)
            {

                MessageBox.Show("The Preview WebView Control has failed and the application has to shut down. Please restart West Wind WebSurge.",
                    "Shutdown Notice",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Environment.Exit(1);
            }
        }
        #endregion

        #region Navigation


        /// <summary>
        /// Navigates the browser to a URL or file
        /// </summary>
        /// <param name="url"></param>
        public virtual void Navigate(string url, bool forceRefresh = false)
        {
            if (string.IsNullOrEmpty(url)) return;

            IsLoaded = false;

            if (forceRefresh)
            {
                WebBrowser.Source = new Uri("about:blank"); //  can't be null, has to be a uri
                WebBrowser.Dispatcher.Invoke(() => WebBrowser.Source = new Uri(url));
                return;
            }

            WebBrowser.Source = new Uri(url);
        }

        /// <summary>
        /// Navigates the browser to a URL or file
        /// </summary>
        /// <param name="uri"></param>
        public virtual void Navigate(Uri uri, bool forceRefresh = false)
        {
            if (uri == null) return;

            IsLoaded = false;
            if (forceRefresh)
            {
                WebBrowser.Source = new Uri("about:blank");  //  can't be null, has to be a uri
                WebBrowser.Dispatcher.Invoke(() => WebBrowser.Source = uri);
                return;
            }
            WebBrowser.Source = uri;
        }


        /// <summary>
        /// Reloads the Web Browser control with the current content
        /// </summary>
        /// <param name="noCache"></param>
        public void Refresh(bool noCache = false)
        {
            IsLoaded = false;
            if (noCache)
            {
                var source = WebBrowser.Source;
                WebBrowser.Source = new Uri("about:blank");  //  can't be null, has to be a uri
                var url = WebBrowser.Source?.ToString();
                if (!string.IsNullOrEmpty(url))
                {
                    WebBrowser.Dispatcher.Invoke(() => WebBrowser.Source = new Uri(url));
                    return;
                }
            }
            
            WebBrowser.CoreWebView2.Reload();
        }

        #endregion
        
        #region ZoomFactor Tracking 
        
        
        /// <summary>
        /// If true, tracks the Zoom factor of this browser instance
        /// over refreshes. By default the WebView looses any user
        /// zooming via Ctrl+ and Ctrl- or Ctrl-Mousewheel whenever
        /// the content is reloaded. This settings explicitly keeps
        /// track of the ZoomFactor and overrides it.
        /// </summary>
        public bool TrackZoomFactor { get; set; }
        
        public double ZoomLevel { get; set;  }= 1;
        private bool CaptureZoom = false;
        
        /// <summary>
        /// Handle keeping the Zoom Factor the same for all preview instances
        /// Without this 
        /// </summary>
        private void CaptureZoomFactor()
        {
            if (WebBrowser.CoreWebView2 == null)
            {
                throw new InvalidOperationException("Please call CaptureZoomFactor after WebView has been initialized");
            }
            
            if (!TrackZoomFactor)
                return;
            
            WebBrowser.ZoomFactorChanged += (s, e) =>
            {
                if (!CaptureZoom)
                {
                    WebBrowser.ZoomFactor = ZoomLevel;
                }
                else
                {
                    ZoomLevel = WebBrowser.ZoomFactor;
                }
            };
            WebBrowser.NavigationStarting += (object sender, CoreWebView2NavigationStartingEventArgs e) => CaptureZoom = false;
            WebBrowser.CoreWebView2.DOMContentLoaded += (s, e) => CaptureZoom = true;
        }

        #endregion

        /// <summary>
        /// Opens the developer tools. If you want tools to open on startup make sure
        /// you call this immediately after instantiating the object.
        /// </summary>
        public void ShowDeveloperTools()
        {
            WebBrowser.CoreWebView2.OpenDevToolsWindow();
        }


        public virtual void Dispose()
        {
            WebBrowser?.Dispose();
        }
    }
}
