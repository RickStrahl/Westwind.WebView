using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Microsoft.Web.WebView2.Wpf
{
    public static class WebView2Extensions
    {
        public static async Task NavigateToStringSafe(this WebView2 webView, string html)
        {
            webView.Source = new Uri("about:blank");

            string encodedHtml = JsonConvert.SerializeObject(html);
            string script = "window.document.write(" + encodedHtml + ")";

            await webView.EnsureCoreWebView2Async();  // make sure WebView is ready
            await webView.ExecuteScriptAsync(script);
        }
    }
}
