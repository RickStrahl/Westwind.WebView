
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using Microsoft.Web.WebView2.Wpf;
using Westwind.Utilities;

namespace WpfSample
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class StringNavigationIssues : MetroWindow
    {
        public StringNavigationIssues()
        {
           

            InitializeComponent();

            //File.WriteAllText("WebContent\\LargeEmbeddedImage.html", html);
            //var file = System.IO.Path.GetFullPath("WebContent\\LargeEmbeddedImage.html");

            WebView.DefaultBackgroundColor = System.Drawing.Color.Silver; 
            WebView.Source = new Uri(Path.GetFullPath("WebContent\\ExportedHtml.html"));
        }

        private void OpenFromString_Click(object sender, RoutedEventArgs e)
        {
            // 3mb string
            var fileContent = HtmlUtils.BinaryToEmbeddedBase64(File.ReadAllBytes(
                "WebContent\\LargeImage.jpg"), "image/jpeg");

            var html =
                $"""
                 <html>
                 <body>
                     <h1>Embedded Large Image</h1>
                     <img src="{fileContent}" style="width: 100%" alt="Large Image" />
                 </body>
                 </html>
                 """;

            WebView.NavigateToString(html);
        }

        private async void OpenFromStringSafe_Click(object sender, RoutedEventArgs e)
        {
            // 3mb string
            var fileContent = HtmlUtils.BinaryToEmbeddedBase64(File.ReadAllBytes(
                "WebContent\\LargeImage.jpg"), "image/jpeg");

            var html =
                $"""
                 <html>
                 <body>
                     <h1>Embedded Large Image</h1>
                     <img src="{fileContent}" style="width: 100%" alt="Large Image" />
                 </body>
                 </html>
                 """;

            await NavigateToString(html);
            //await WebView.NavigateToStringSafe(html);
        }

        private void OpenFromStringFile_Click(object sender, RoutedEventArgs e)
        {
            var file = System.IO.Path.GetFullPath("WebContent\\RenderedHtml.html");
            // 3mb string
            var fileContent = HtmlUtils.BinaryToEmbeddedBase64(File.ReadAllBytes(
                "WebContent\\LargeImage.jpg"), "image/jpeg");

            var html =
                $"""
                 <html>
                 <body>
                     <h1>Embedded Large Image</h1>
                     <img src="{fileContent}" style="width: 100%" alt="Large Image" />
                 </body>
                 </html>
                 """;

            NavigateToStringFile(html);
        }

        async Task NavigateToString(string html)
        {
            WebView.Source = new Uri("about:blank");

            string encodedHtml = JsonConvert.SerializeObject(html);
            string script = "window.document.write(" + encodedHtml + ")";

            await WebView.EnsureCoreWebView2Async();  // make sure WebView is ready
            await WebView.ExecuteScriptAsync(script);
        }

        void NavigateToStringFile(string html) 
        {
            var file = Path.Combine(Path.GetTempPath(), "MyApp_RenderedHtml.html");
            File.WriteAllText(file, html);
            WebView.Source = new Uri(file);
        }
 
    }

}
