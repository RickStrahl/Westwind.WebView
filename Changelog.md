# Westwind.WebView Changelog


### 0.2

* **Merge Westwind.WebView.HtmlToPdf into Library**  
Moved the HtmlToPdf generation library into this library to reduce multiple runtime mapping requirements if both libraries are used. Both libraries share the runtime reference and some support code.

* **Add .NET 9.0 target, drop .NET 6.0 target**  
With release of .NET 9.0 we're adding the new release target and dropping off the old out of maintenance version of .NET.

### 0.1.22

* **Rollback to previously used runtime (temporary)**  
Due to a targeting bug in the WebView2 SDK, I'm rolling back to `v1.0.2592.51` of the SDK. Later versions prior to `.2800` will not push forward the transitive WebView sdk dependencies into the top level projects so an explicit reference to `Microsoft.Web.WebView2` was required. I'll update as soon as that bug has been fixed.

### 0.1.21 

* **Update to latest WebView Runtime**

### 0.1.15

* **Add support for `NavigateToString()` and `NavigateFromStream()` with large Content**  
The native `NavigateToString()` is [restricted to HTML content of less than 2mb](https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.navigatetostring?view=webview2-dotnet-1.0.2045.28#remarks) and has no stream support. These functions avoid the limitation and add stream support.