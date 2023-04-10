using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Microsoft.Web.WebView2.Wpf;
using Westwind.WebView.Wpf;

namespace WpfSample
{
    /// <summary>
    /// Interaction logic for BasicInterop.xaml
    /// </summary>
    public partial class BasicInterop : MetroWindow
    {
        BasicInteropWebviewHandler WebViewHandler { get;  }
        
        
        public BasicInterop()
        {
            InitializeComponent();
            ThemeOverride.SetThemeWindowOverride(this, "Dark");

            Model = new BasicInteropModel();
            DataContext = Model;

            Loaded += BasicInterop_Loaded;


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
            var url = "https://WebViewSample.basicinterop/Interop.html";
            url += "?mode=dark"; // pass dark theme in  (light without this)

            WebViewHandler = new BasicInteropWebviewHandler(WebBrowser,
                System.IO.Path.Combine(System.IO.Path.GetTempPath(), "WpfSample_WebView"))
            {
                // virutal host name for the folder
                HostWebHostNameForFolder = "WebViewSample.basicinterop",
                HostWebRootFolder = previewPath,

                // Initial page to load after loading is complete - ensures no invalid URL before host is assigned
                InitialUrl = url
            };

        }

        public BasicInteropModel Model  { get; set; }

        private void BasicInterop_Loaded(object sender, RoutedEventArgs e)
        {
            if (Owner != null)
            {
                Top = Owner.Top + 65;
                Left = Owner.Left + 60;
            }
        }
    }

    #region Model Data

    public class BasicInteropModel : INotifyPropertyChanged
    {
        public Person Person { get; set;  } = new Person();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }


    public class Person : INotifyPropertyChanged
    {
        private string _firstname = "Billy";
        private string _lastname = "Bopp";
        private string _email = "bopp@beebopp.com";
        private string _company = "beebob.com";
        private Address _address = new Address();

        public string Firstname
        {
            get => _firstname;
            set
            {
                if (value == _firstname) return;
                _firstname = value;
                OnPropertyChanged();
            }
        }

        public string Lastname
        {
            get => _lastname;
            set
            {
                if (value == _lastname) return;
                _lastname = value;
                OnPropertyChanged();
            }
        }

        public string Company

        {
            get => _company;
            set
            {
                if (value == _company) return;
                _company = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (value == _email) return;
                _email = value;
                OnPropertyChanged();
            }
        }

        public Address Address
        {
            get => _address;
            set
            {
                if (Equals(value, _address)) return;
                _address = value;
                OnPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class Address : INotifyPropertyChanged
    {
        private string _street = "123 Nowher Lane";
        private string _city = "Anytown";
        private string _state = "UT";
        private string _country = "USA";
        private string _postalCode = "12314";

        public string Street
        {
            get => _street;
            set
            {
                if (value == _street) return;
                _street = value;
                OnPropertyChanged();
            }
        }

        public string City
        {
            get => _city;
            set
            {
                if (value == _city) return;
                _city = value;
                OnPropertyChanged();
            }
        }

        public string State
        {
            get => _state;
            set
            {
                if (value == _state) return;
                _state = value;
                OnPropertyChanged();
            }
        }

        public string PostalCode

        {
            get => _postalCode;
            set
            {
                if (value == _postalCode) return;
                _postalCode = value;
                OnPropertyChanged();
            }
        }

        public string Country
        {
            get => _country;
            set
            {
                if (value == _country) return;
                _country = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    #endregion


    #region WebView Handler

    public class BasicInteropWebviewHandler : EmojiWebViewHandler
    {
        public BasicInteropWebviewHandler(WebView2 webViewBrowser, string webViewEnvironmentFolder = null,
            object dotnetCallbackObject = null) :
            base(webViewBrowser, webViewEnvironmentFolder, dotnetCallbackObject)
        {
            JsInterop = new EmojiWebViewInterop(webViewBrowser);
            HostObject = JsInterop;
        }

   
    }


    public class BasicInteropWebViewInterop : BaseJavaScriptInterop
    {
        public BasicInteropWebViewInterop(WebView2 webView) : base(webView, "window.page")
        {

        }

    }

    #endregion
}
