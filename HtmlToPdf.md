# .NET Html to Pdf Conversion using WebView on Windows

#### *Creating Pdf from Html with .NET on Windows using the WebView2 control*


| Library        | Nuget Package          |
|----------------|----------------|
| Westwind.WebView | [![](https://img.shields.io/nuget/v/Westwind.WebView.svg)](https://www.nuget.org/packages/Westwind.WebView/) [![](https://img.shields.io/nuget/dt/Westwind.WebView.svg)](https://www.nuget.org/packages/Westwind.WebView/) |
| ~~Westwind.WebView.HtmlToPdf (deprecated)~~ | [![](https://img.shields.io/nuget/v/Westwind.WebView.HtmlToPdf.svg)](https://www.nuget.org/packages/Westwind.WebView.HtmlToPdf/) [![](https://img.shields.io/nuget/dt/Westwind.WebView.HtmlToPdf.svg)](https://www.nuget.org/packages/Westwind.WebView.HtmlToPdf/) |

> [!IMPORTANT]
> The old `Westwind.WebView.HtmlToPdf` library has been deprecated and has been rolled into this `Westwind.WebView` library. All features and syntax carry over from the old version and the library can be used without changes except swapping out the NuGet package reference.


This library provides a quick way to print Html to Pdf on Windows using the WebView control. You can generate Pdf from Html of a Url or file by using a few different mechanisms:

* To File
* To Stream
* Using an Async Call
* Using Event Callbacks

The base library uses the built-in **WebView2 Runtime in Windows so it has no external dependencies for your applications** assuming you are running on a recent version of Windows that has the WebView2 Runtime installed. The extended package version provides additional features, but it also has additional dependencies and is slower to run and has a larger distribution footprint. In order to keep the base functionality very small and lean these two packages have been separated.


If you would like to find out more how this library works and how the original code and Pdf code was build, you can check out this blog post here:

* [Programmatic Html to Pdf Generation using the WebView2 Control](https://weblog.west-wind.com/posts/2024/Mar/26/Programmatic-Html-to-PDF-Generation-using-the-WebView2-Control-with-NET)

## Prerequisites
The library is Windows specific, but it works in a variety of environments on Windows.

### Support for
* Windows 11/10 Server 2019/2022
* Desktop Applications
* Console Applications
* Service Applications

The component does not support:

* Non Windows platforms

### Targets

* net9.0-windows
* net8.0-windows
* net472

### Dependencies
Deployed applications have the following dependencies:  

* [WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/?form=MA13LH&ch=1#download)  
On recent updates of Windows 11 and 10, the WebView is pre-installed as a system component. On Servers however, you may have to explicitly install the WebView Runtime.

* [Windows Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  
The WebView2 component is dependent on Windows Desktop Runtime libraries and therefore requires the Desktop runtime to be installed **even for server applications**. 

## Using the library
This library only has a single dependency on the WebView control and provides very fast base Html to PDF conversion. This library is lean and fast and does just base Pdf conversion.

You can install this NuGet package:

```ps
dotnet add package westwind.webview
```

The library exposes 4 separate output methods:

* PrintToPdfStreamAsync() - Runs async and returns a `result.ResultStream`
* PrintToPdfAsync() - Runs async and creates a PDF output file
* PrintToPdfStream() - Creates a PDF and returns it via Callbacks in `result.ResultStream` 
* PrintToPdf()  - Creates a PDF to file a notifies completion via Callback

All of the methods take a file or Url as input. File names have to be fully qualified with a path. Output to file requires that you provide a filename.

All requests return a `PdfPrintResult` structure which has a `IsSuccess` flag you can check. For stream results, the `ResultStream` property will be set with a `MemoryStream` instance on success. Errors can use the `Message` or `LastException` to retrieve error information.

### Async Call Syntax for Stream Result

```cs
var outputFile = Path.GetFullPath(@".\test3.pdf");
var htmlFile = Path.GetFullPath("HtmlSampleFileLonger-SelfContained.html");

var host = new HtmlToPdfHost()
{
    BackgroundHtmlColor = "#ffffff"
};
host.CssAndScriptOptions.KeepTextTogether = true;            

var pdfPrintSettings = new WebViewPrintSettings()
{
    // margins are 0.4F default
    MarginTop = 0.5,
    MarginBottom = 0.3F,
    ScaleFactor = 0.9F,   // 1 is default

    ShouldPrintHeaderAndFooter = true,
    HeaderTitle = "Custom Header (centered)",
    FooterText = "Custom Footer (lower right)",

    // Optionally customize the header and footer completely - WebView syntax                
    // HeaderTemplate = "<div style='text-align:center; font-size: 12px;'>" + 
    //                  "<span class='title'></span></div>",
    // FooterTemplate = "<div style='text-align:right; margin-right: 2em'>" + 
    //                  "<span class='pageNumber'></span> of " +
    //                  "<span class='totalPages'></span></div>",

    GenerateDocumentOutline = true  // default
};

// We're interested in result.ResultStream
var result = await host.PrintToPdfStreamAsync(htmlFile, pdfPrintSettings);

Assert.IsTrue(result.IsSuccess, result.Message);
Assert.IsNotNull(result.ResultStream); // THIS

// Copy resultstream to output file
File.Delete(outputFile);
using (var fstream = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.Write))
{
    result.ResultStream.CopyTo(fstream);
    result.ResultStream.Close(); // Close returned stream!
}
ShellUtils.OpenUrl(outputFile);
```

### Async Stream Example in a Web Application

```csharp
[HttpGet("rawpdf")]
public async Task<IActionResult> RawPdf()
{
    // source file or URL to render to Pdf
    var file = Path.GetFullPath("./HtmlSampleFile-SelfContained.html");

    var pdf = new HtmlToPdfHost();
    var pdfResult = await pdf.PrintToPdfStreamAsync(file, new WebViewPrintSettings {  PageRanges = "1-10"});

    if (pdfResult == null || !pdfResult.IsSuccess)
    {
        Response.StatusCode = 500;                
        return new JsonResult(new
        {
            isError = true,
            message = pdfResult.Message
        });
    }

    return new FileStreamResult(pdfResult.ResultStream, "application/pdf");             
}
```

### Async Call Syntax for File Output

```csharp
// Url or full qualified file path
var htmlFile = Path.GetFullPath("HtmlSampleFileLonger-SelfContained.html");
var outputFile = Path.GetFullPath(@".\test2.pdf");
File.Delete(outputFile);

var host = new HtmlToPdfHost(); // or new HtmlToPdfHostExtended()
var result = await host.PrintToPdfAsync(htmlFile, outputFile);

Assert.IsTrue(result.IsSuccess, result.Message);
ShellUtils.OpenUrl(outputFile);  // display the Pdf file you specified
```


### Callback Syntax to Pdf File

```csharp
var htmlFile = Path.GetFullPath("HtmlSampleFile-SelfContained.html");
var outputFile = Path.GetFullPath(@".\test.pdf");
File.Delete(outputFile);

var host = new HtmlToPdfHost();            

// Callback when complete
var onPrintComplete = (PdfPrintResult result) =>
{
    if (result.IsSuccess)
    {
        ShellUtils.OpenUrl(outputFile);
        Assert.IsTrue(true);
    }
    else
    {
        Assert.Fail(result.Message);
    }
};
var pdfPrintSettings = new WebViewPrintSettings()
{
    // default margins are 0.4F
    MarginBottom = 0.2F,
    MarginLeft = 0.2f,
    MarginRight = 0.2f,
    MarginTop = 0.4f,
    ScaleFactor = 0.8f,
    PageRanges = "1,2,5-7"
};
host.PrintToPdf(htmlFile, outputFile, onPrintComplete, pdfPrintSettings);

// make sure app keeps running
```

### Event Syntax to Stream

```csharp
// File or URL
var htmlFile = Path.GetFullPath("HtmlSampleFile-SelfContained.html");                       
var host = new HtmlToPdfHost();

// Callback on completion
var onPrintComplete = (PdfPrintResult result) =>
{
    if (result.IsSuccess)
    {
        // create file so we can display
        var outputFile = Path.GetFullPath(@".\test1.pdf");
        File.Delete(outputFile);

        using var fstream = new FileStream(outputFile, FileMode.OpenOrCreate, FileAccess.Write);
        result.ResultStream.CopyTo(fstream);

        result.ResultStream.Close(); // Close returned stream!

        ShellUtils.OpenUrl(outputFile);
        Assert.IsTrue(true);
    }
    else
    {
        Assert.Fail(result.Message);
    }
};
var pdfPrintSettings = new WebViewPrintSettings()
{
    MarginBottom = 0.2F,
    MarginLeft = 0.2f,
    MarginRight = 0.2f,
    MarginTop = 0.4f,
    ScaleFactor = 0.8f,
};
host.PrintToPdfStream(htmlFile, onPrintComplete, pdfPrintSettings);

// make sure app keeps running
```

The `Task` based methods are easiest to use so that's the recommended syntax. The callback based methods are there so you can more easily use this if you are running in a non-async and can't easily transition to async. 

Both approaches run on a separate STA thread to ensure that the WebView can run regardless of whether you are running inside of an application that has a main UI/STA thread and it works inside of Windows Service contexts.

## FAQ

### Does it work it work in non-Interactive Environments like a Web Application
Yes, but you have to be careful that you have permissions to retrieve any files you might be rendering.

The following demonstrates running this library in an ASP.NET Core application in IIS using the ANCM using the standard `ApplicationPoolIdentity` account.

![Running Under IIS](Assets/RunningUnderIIS.png)

But as is always the case you have to make sure you have the right permissions to access the files you want to convert and if you're generating to a file that  you have write access. Generally you'll want to generate to stream.

You can look at the [AspNetSample](/AspNetSample) which contains an ASP.NET Core test project that demonstrates Web usage. To see the permissions issues though you have to run IIS using a default or non-interactive account.

To return a PDF as a document in a Controller:

```csharp
[HttpGet("rawpdf")]
public async Task<IActionResult> RawPdf()
{
    var file = Path.GetFullPath("./HtmlSampleFile-SelfContained.html");

    var pdf = new HtmlToPdfHost();
    var pdfResult = await pdf.PrintToPdfStreamAsync(file, new WebViewPrintSettings {  PageRanges = "1-10"});

    if (pdfResult == null || !pdfResult.IsSuccess)
    {
        Response.StatusCode = 500;                
        return new JsonResult(new
        {
            isError = true,
            message = pdfResult.Message
        });
    }

    return new FileStreamResult(pdfResult.ResultStream, "application/pdf");             
}
```

In this scenario I'm running **ApplicationPoolIdentity** for the Application Pool and there are no special permissions given.


## Support us
If you use this project and it provides value to you, please consider supporting by contributing or supporting via the sponsor link or one time donation at the top of this page. Value for value.
