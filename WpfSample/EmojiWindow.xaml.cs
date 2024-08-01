using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Westwind.WebView.Utilities;
using Westwind.WebView.Wpf;


namespace WpfSample
{
    /// <summary>
    /// Example that demonstrates hosting a WebView control and
    /// interacting with script code in the page:
    ///
    /// * Search Term is captured in WPF
    /// * Value is passed into JavaScript (JsInterop)
    /// * JavaScript searches and filters the list displayed
    /// * Click is handled in JavaScript
    /// * Result is passed back out into WPF (JsInterop)
    /// 
    /// </summary>
    public partial class EmojiWindow : MetroWindow, INotifyPropertyChanged
    {
        public bool Cancelled { get; set; }
        
        /// <summary>
        /// Search text is captured in WPF and passed into JavaScript
        ///
        /// Click is captured in JavaScript and fires event in WPF
        /// </summary>
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (value == _searchText) return;
                WebViewHandler.InitialSearchText = value;

                _searchText = value;
                OnPropertyChanged();
            }
        }

        private string _searchText;

        public EmojiWebViewHandler WebViewHandler { get; set; }


        public string EmojiString
        {
            get { return _emojiString; }
            set
            {
                if (value == _emojiString) return;
                _emojiString = value;
                OnPropertyChanged(nameof(EmojiString));
            }
        }

        private string _emojiString;


        public EmojiWindow()
        {

            InitializeComponent();

            DataContext = this;
            ThemeOverride.SetThemeWindowOverride(this, "Dark");

#if DEBUG
            // for debug use your actual dev source path for the HTML content so you can F5 reload without restart
            var previewPath = @"d:\projects\Libraries\Westwind.WebView\WpfSample\PreviewThemes";
            if (!Directory.Exists(previewPath))
            {
                previewPath = System.IO.Path.Combine(App.InitialStartDirectory, "PreviewThemes");   // production folder
            }
#else
            // Production uses the runtime location which only updates on compile (unless you change in place)
            var previewPath = System.IO.Path.Combine(App.InitialStartDirectory,"PreviewThemes");   // production folder
#endif

            // page to load in WebView
            var url = "https://markdownmonster.emoji/_EmojiViewer.html";
            url += "?mode=dark"; // pass dark theme in  (light without this)

            WebViewHandler = new EmojiWebViewHandler(WebBrowser,
                System.IO.Path.Combine(Path.GetTempPath(), "WpfSample_WebView"))
            {
                // virutal host name for the folder
                HostWebHostNameForFolder = "markdownmonster.emoji",
                HostWebRootFolder = previewPath,
                ShowDevTools = false,  // show dev tools on startup

                // if using a custom interop handler assign and configure here
                JsInterop = new EmojiWebViewInterop(WebBrowser),

                // Initial page to load after loading is complete - ensures no invalid URL before host is assigned
                InitialUrl = url
            };

            Loaded += EmojiWindow_Loaded;

           
        }

        private async void EmojiWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
            {
                Top = Owner.Top + 45;
                Left = Owner.Left + 30;
            }
            TextSearchText.Focus();       
        }

        private void EmojiWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Cancelled = true;
                Close();
            }
        }

        private void TextSearchText_Keydown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Escape)
            {
                if (!string.IsNullOrEmpty(SearchText))
                {
                    SearchText = null;
                    e.Handled = true;
                }
            }



        }

        private void TextSearchText_Keyup(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                WebViewHandler.JsInterop.SearchEmoji(SearchText).FireAndForget();
            }

            WebViewHandler.JsInterop.SearchEmoji(SearchText).FireAndForget();
        }



        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void SelectEmoji(string emojiValue)
        {
            EmojiString = emojiValue;
            if (!DialogResult.HasValue)
                DialogResult = true;
            Close();
        }

        private void BtnDebugger_OnClick(object sender, RoutedEventArgs e)
        {
            WebViewHandler.WebBrowser?.CoreWebView2?.OpenDevToolsWindow();
        }
    }


    #region WebView Handler


    /// <summary>
    /// Custom WebView Handler Implementation that overrides some default behaviors
    ///
    /// Subclassing here allows you to isolate the WebView specific behaviors
    /// from the main part of your form. You can pass in relevant data, or
    /// your model, or form here if necessary to get access to your app state.
    /// </summary>
    public class EmojiWebViewHandler : WebViewHandler<EmojiWebViewInterop>
    {
        public string InitialSearchText { get; set;  }
        

        public EmojiWebViewHandler(WebView2 webViewBrowser, string webViewEnvironmentFolder = null,
            object dotnetCallbackObject = null) :
            base(webViewBrowser, webViewEnvironmentFolder, dotnetCallbackObject)
        {
            //JsInterop = new EmojiWebViewInterop(webViewBrowser);
            HostObject = JsInterop;
        }

        protected override async void OnDomContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            base.OnDomContentLoaded(sender,e);

            if (!string.IsNullOrEmpty(InitialSearchText))
            {
                await JsInterop.SearchEmoji(InitialSearchText);
            }
        }
    }

    /// <summary>
    /// This class handles all interop between JavaScript and .NET. Typically
    /// I would break these out into two classes for .NET -> JavaScript and JavaScript->.NET
    /// but since we have only 1 method each a single class suffices to handle both.
    ///
    /// The JS Interop allows making `Invoke`  calls to call into JavaScript methods using
    /// provided base interop target in JS code (in this case `window.page`) on which methods
    /// are invoked. Invoke calls allow passing complex data which is serialized into JSON
    /// automatically.
    ///
    /// Callbacks are just methods that receive results from JS code. Callback parameters
    /// need to be simple type values and any complex data has to be passed back as JSON.
    /// </summary>
    public class EmojiWebViewInterop : BaseJavaScriptInterop
    {
        /// <summary>
        /// must implement this constructor!
        /// </summary>
        /// <param name="webView"></param>
        /// <param name="baseInvocationTarget"></param>
        public EmojiWebViewInterop(WebView2 webView, string baseInvocationTarget = "window.page" ) : base(webView, baseInvocationTarget)
        {

        }
    
        
        /// <summary>
        /// All calls into JS code are async!
        /// </summary>
        /// <param name="searchText"></param>
        /// <returns></returns>
        public async Task SearchEmoji(string searchText)
        {
            await Invoke("searchEmoji", searchText);
        }

        
        
        /// <summary>
        /// Callback that receives the selected Emoji value on a click,
        /// </summary>
        /// <param name="emojiValue"></param>
        public void EmojiUpdated(string emojiValue)
        {
            var window = WebBrowser.TryFindParent<Window>() as EmojiWindow;
            window.SelectEmoji(emojiValue);
        }


    }

    #endregion
}
