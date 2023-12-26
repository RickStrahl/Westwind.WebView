# Westwind.WebView Interop Helpers

A .NET support library for the `Microsoft.Web.WebView2` control to aid with common operations and .NET / JavaScript interop.

> This is an internal library and I'm opening this up for reference in various posts and support tools. It's not well documented at this time. 

The library provides:

* A base behavior `WebViewHandler` class that wraps initialization and events  
and provides built in Interop support
* A JavaScript Interop class that simplifies calling into JavaScript from .NET
* [A CachedWebView Environment to ensure consistent Environment reuse](https://weblog.west-wind.com/posts/2023/Oct/31/Caching-your-WebView-Environment-to-manage-multiple-WebView2-Controls)

The WebView Handler is meant to be used when you need to do a lot of Interop between your .NET and JavaScript code. It ties together the WebView initialization, calling of methods in JavaScript and receiving callbacks back into .NET from JavaScript and hooking common events that you might have to deal with.

## Installation
To install the library install the NuGet package from:

```ps
install-package Westwind.WebView
```

## WebViewHandler Usage
The WebView Handler is primarily meant to be used when you need to do a lot of Interop between your .NET and JavaScript code. It ties together the WebView initialization, calling of methods in JavaScript and receiving callbacks back into .NET from JavaScript. Initialization initializes the WebView but also provides hooks for when content has loaded so you can start running JavaScript code and pass in state when the WebView initially loads.

There are three distinct components:

* **The `WebViewHandler`**  
This is the top level object that handles WebView initialization, setting up things like mapping a local file path to a Web domain (if needed), hooking up a .NET callback object that can be called from .NET and creating an instance of a JavaScript proxy that allows more easily calling into JavaScript.

* **The JavaScript Interop Object**  
This is a class that acts as an RPC proxy into .NET that basically helps you make `ExecuteScriptAsync()` calls into .NET by automatically handling parameter serialization and result deserialization. Using a Reflection like interface that lets you use `Invoke()` and `Get<T>()`, `Set<T>()` methods to interop with JavaScript code.

* **A Dotnet Callback Object**  
This objects is 'passed into JavaScript' and accessible as a host object in JavaScript via:
	
	```js
	 // Async
	 let result = await window.chrome.webview.hostObjects.websurge.RunRequest(url);
	 
	 // Sync
	 let success = window.chrome.webview.hostObjects.sync.websurge.NavigateLink(url);
	```

	A host object is just a .NET POCO object that contains methods to callback to either sync or async. You don't need to provide this if you don't have callbacks, or if you have only very few you can just use the JavaScriptInterop object to hold those methods and pass that.
	
	> Both the JavaScript and Dotnet objects are optional.  You can pass those in as null values and they won't be set or used for anything, but if that's the case you probably don't have much need for the `WebViewHandler` in the first place.

* **A Cached WebView Environment Class**  
This class handles consistently initializing and re-using a single WebView environment inside of an application to avoid odd behaviors due to incompatible environment settings. Avoids among other things creating a default environment which may not work in some application if permissions don't allow access to create the default environment folder. The static methods of this class are used by default by the `WebViewHandler` to initialize the environment via `CoreWebView2Environment.CreateAsync()`.

`WebViewHandler` is a behavior class that is attached to an existing instance of a WebView control, typically assigned in the constructor of the WebView host control.

The recommended way to use these tools is:

* Create an application specific subclass for `WebViewHandler` and - as needed - `JavaScriptInterop` and `DotnetInterop` objects
* Attach the behavior to a WebView control in the host's control or form CTOR.

### Creating Application Specific WebViewHandlers
The recommended way to use these classes is by deriving an application specific subclass for the `WebViewHandler`, and if needed `JavaScriptInterop` and `DotnetInterop` objects:

```csharp
/// <summary>
/// Create an application specific implementation of the WebView Handler
///
/// CTOR/base configures the optional dependencies for the JavaScript and Dotnet
/// interop objects.
/// </summary>
public class DocumentationPreviewHandler : WebViewHandler<DocumentationPreviewJavaScriptInterop>
{
    public DocumentationPreviewHandler(WebView2 webBrowser) :
        base(webBrowser, wsApp.Constants.WebViewEnvironmentFolderName, new DocumentationPreviewDotnetInterop())
    {
        
    }
}


/// <summary>
/// Subclass from the BaseJavaScriptInterop class to get the abililty to easily
/// call methods in the JavaScript code.
///
/// Recommend you create a method for each JavaScript call you make using `Invoke()`
/// </summary>
public class DocumentationPreviewJavaScriptInterop : BaseJavaScriptInterop
{
    public DocumentationViewer DocumentationViewer { get; set; }

    public DocumentationPreviewJavaScriptInterop(WebView2 webBrowser, string baseInvocationTarget = "window") : base(webBrowser,baseInvocationTarget)
    {
    }


    /// <summary>
    /// Update the document with an HTML string. Optional line number
    /// on where to scroll the document to.
    /// </summary>
    /// <param name="html"></param>
    /// <param name="lineNo"></param>
    public async Task UpdateDocumentContent(string html, int lineNo)
    {
        await Invoke("updateDocumentContent", html, lineNo);
    }


    /// <summary>
    /// Scroll to a specific line in the document
    /// </summary>
    public async Task ScrollToPragmaLine(int editorLineNumber = -1,
        string headerId = null,
        bool updateCodeBlocks = true,
        bool noScrollTimeout = false, bool noScrollTopAdjustment = false)
    {
        await Invoke("scrollToPragmaLine",
            editorLineNumber, headerId,
            noScrollTimeout, noScrollTopAdjustment);
    }
}


/// <summary>
/// If you have a lot of callbacks use a separate object.
/// Otherwise you may just use the JavaScript object above
/// to send callbacks to.
/// 
/// This is a plain .NET object - keep it simple as this it
/// uses COM for its marshaling.
/// </summary>
[ComVisible(true)]
public class DocumentationPreviewDotnetInterop
{
}
```

### Using the Custom WebViewHandler
Once you've created the handler you can then assign it to a Web View control.

In its simplest for you can just instantiate the handler:

```cs
// Host control
public partial class DocumentationViewer : UserControl
{
    DocumentationViewerModel Model { get;  }    

    public DocumentationPreviewHandler PreviewHandler { get; set; }

    // do this either in the CTOR or Loaded 
	private void DocumentationViewer_Loaded(object sender, System.Windows.RoutedEventArgs e)
	{
       var jsInterop = new DocumentationPreviewJavaScriptInterop(PreviewBrowser, "window")
        {
            DocumentationViewer =  this   // custom state 
        };
        PreviewHandler = new DocumentationPreviewHandler(PreviewBrowser)
        {
           JsInterop = jsInterop // optional
        };
	}
}
```

Something a little more sophisticated might look like this where we specify a host of additional settings:


```csharp
void ConfigureEditor(RequestDocumentationItem documentation)
{
    var dotnetHostObject = new DocumentationEditorDotnetHostObject(AppModel.Current, this, null)
    {
        DocItem = documentation
    };
    var jsInterop = new DocumentationEditorJavaScriptInterop(EditorBrowser, "window.textEditor");
    //Loaded += DocumentationViewer_Loaded;

#if DEBUG
    //var editorPath = "Editor";   // production folder
    var editorPath = @"d:\projects\WebSurge2\WebSurge\Html\Editor";
#else
    var editorPath = System.IO.Path.GetFullPath(".\\HTML\\Editor");   // production folder
#endif
    EditorHandler = new DocumentationEditorWebViewHandler(EditorBrowser, dotnetHostObject)
    {
        JsInterop = jsInterop,
        HostObjectName = "mm",   // HostObject name inside of WebView
        ShowDevTools = false,
        
        HostWebRootFolder = editorPath,                   // folder used as web site
        HostWebHostNameForFolder = "websurge.doceditor",  // mapped domain
        InitialUrl = "https://websurge.doceditor/editor.htm"
    };
    
    // additional app specific properties in custom version that are used for initial nav
    EditorHandler.InitialValue = documentation.Documentation;  // custom logic applied
}
```

This initial assignment triggers the initialization of the WebView and essentially starts an initial navigation with the assigned `InitialUrl` (or `Source` if not assigned). InitialUrl is a delayed navigation that ensures that the URL is not set until after the Host folder is mapped. This avoids failed navigations on initial display of the WebView.


## CachedWebViewEnvironment Usage
This class can be used to initialize the WebView Environment consistently. It's also used internally by `WebViewHandler` if no explicit environment is passed set in the CTOR.

This class can be used independently of `WebViewHandler` if you manually instantiate your WebView environment.

### Initialize Environment Folder and Options
The first step is to initialize the WebView Environment folder location and set any options. This should be done **before the WebView is first instantiated** preferably during startup of the application.

In WPF `OnStartup()` is a good place:

```csharp
protected override void OnStartup(StartupEventArgs e)
{

    // initialize single environment folder for all WebViews
    CachedWebViewEnvironment.Current.EnvironmentFolderName = Path.Combine(
       mmApp.Configuration.LocalAppDataFolder,
       mmApp.Constants.WebViewEnvironmentFolderName);
       
    // Optionally - set any custom startup flags and options.
    //             Typically this can be left at null
    // CachedWebViewEnvironment.Current.EnvironmentOptions = null;

    ...
}
```

### Initializing the WebView Control
Then, anywhere you need to use a WebView Environment, you can then initialize the WebView with this environment via the `InitializeWebViewEnvironment()` method, which either creates a new environment if it doesn't exist yet, or reuses the previously created one that is cached.

This method calls `webBrowser.EnsureCoreWebView2Async()` to wait for the WebView to be initialized **and become UI active** (!) using the cached environment as its parameter.

> Note the `EnsureCoreWebView2Async()` and by extension `InitializeWebViewEnvironment()` can take a long time to complete as it waits for UI activation before returning. 
>
> If the WebView is not visible (ie. inactive on another tab, or otherwise not visible) it will not return until it becomes active.

In a usage scenario you can use `InitializeWebViewEnvironment()` like this during WebView initialization:

```cs
// Manual WebView Initialization
protected async Task InitializeAsync()
{
    if (JsInterop == null)
        JsInterop = CreateJsInteropInstance();
    
    if (!IsInitialized)  // Ensure this doesn't run more than once
    {
        // THIS
        await CachedWebViewEnvironment.Current.InitializeWebViewEnvironment(WebBrowser);
    
        if(InitializeComplete != null)
            InitializeComplete();   
    }
    ...          
    // Code to set up Virtual Folder mapping
    // initial navigation etc.
}            
```

Rinse and repeat this process if you have multiple WebView controls in your application, or if you are repeatedly creating the same control.