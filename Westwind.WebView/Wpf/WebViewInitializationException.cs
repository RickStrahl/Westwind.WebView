using System;

namespace Westwind.WebView.Wpf
{
    public class WebViewInitializationException : Exception
    {
        public WebViewInitializationException()
        { }
        public WebViewInitializationException(string message) : base(message)
        { }
        public WebViewInitializationException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}