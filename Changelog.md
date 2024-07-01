# Westwind.WebView Changelog

### 1.15

* **Add support for `NavigateToString()` and `NavigateFromStream()` with large Content**  
The native `NavigateToString()` is [restricted to HTML content of less than 2mb](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.navigatetostring?view=webview2-dotnet-1.0.2045.28#remarks) and has no stream support. These functions avoid the limitation and add stream support.