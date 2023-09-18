using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Westwind.WebView.Wpf
{

    /// <summary>
    /// Helper class that simplifies calling JavaScript functions
    /// in a WebView document. Helps with formatting calls using
    /// ExecuteScriptAsync() and properly formatting/encoding parameters.
    /// It provides an easy way to Invoke methods on a global
    /// object that you specify (ie `window` or some other object) using
    /// a Reflection like Invocation interface.
    ///
    /// For generic Window function invocation you can use this class
    /// as is.
    /// 
    /// But we recommend that you subclass this class for your application
    /// and then implement wrapper methods around each interop call
    /// you make into the JavaScript document rather than making the
    /// interop calls in your application code.
    /// 
    /// Operations are applied to the `BaseInvocationTarget` which is
    /// the base object that operations are run on. This can be the
    /// `window.` or any globally accessibly object ie. `window.textEditor.`.
    ///
    /// You can use Invoke, Set, Get to access object properties, or you can
    /// use `ExecuteScriptAsync` to fire raw requests.
    ///
    /// Parameterize helps with encoding parameters when calling methods
    /// and turning them into string parseable values using JSON.
    ///
    /// *** IMPORTANT ***
    /// When subclassing make sure you create the class with
    /// ***the same constructor signature as this class***
    /// and preferrably pre-set the `baseInvocationTarget` parameter
    /// (`window` or `window.someObject`)   
    /// </summary>
    public class BaseJavaScriptInterop
    {
        /// <summary>
        /// A string that is used as the object for object invocation
        /// By default this is empty which effectively invokes objects
        /// in the root namespace (window.). This string should reflect
        /// a base that supports appending a method or property acces,
        /// meaning it usually will end in a `.` (except for root/empty)
        /// such as `object.property.`
        ///
        /// For other components this might be an root object. In the MM
        /// editor for example it's:
        ///
        /// `window.textEditor.`
        /// </summary>
        public string BaseInvocationTargetString { get; set; }

        /// <summary>
        /// WebBrowser instance that the interop object operates on
        /// </summary>
        public WebView2 WebBrowser { get; }


        /// <summary>
        /// Creates an instance of this interop object to call JavaScript functions
        /// in the loaded DOM document.
        /// </summary>
        /// <param name="webBrowser"></param>
        /// <param name="targetInvocationTarget">The base 'object' to execute
        /// commands on. The default is the global `window.` object. Set with the
        /// `.` at the end.</param>
        public BaseJavaScriptInterop(WebView2 webBrowser, string baseInvocationTarget = "window")
        {
            WebBrowser = webBrowser;
            if (string.IsNullOrEmpty(baseInvocationTarget))
                baseInvocationTarget = "window.";
            if (!baseInvocationTarget.TrimEnd().EndsWith("."))
                baseInvocationTarget += ".";

            BaseInvocationTargetString = baseInvocationTarget;
        }


        #region Serialization

        static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };


        /// <summary>
        /// Helper method that consistently serializes JavaScript with Camelcase
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SerializeObject(object data)
        {
            return JsonConvert.SerializeObject(data, _serializerSettings);
        }

        /// <summary>
        /// Helper method to deserialize json content
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static TResult DeserializeObject<TResult>(string json)
        {
            return JsonConvert.DeserializeObject<TResult>(json, _serializerSettings);
        }

        #endregion


        #region Async Invocation Utilities

        /// <summary>
        /// Calls a method with simple or no parameters: string, boolean, numbers
        /// </summary>
        /// <param name="method">Method to call</param>
        /// <param name="parameters">Parameters to path or none</param>
        /// <returns>object result as specified by TResult type</returns>
        public async Task<TResult> Invoke<TResult>(string method, params object[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(BaseInvocationTargetString + method + "(");

            if (parameters != null)
            {
                for (var index = 0; index < parameters.Length; index++)
                {
                    object parm = parameters[index];
                    var jsonParm = SerializeObject(parm);
                    sb.Append(jsonParm);
                    if (index < parameters.Length - 1)
                        sb.Append(",");
                }
            }
            sb.Append(")");

            var cmd = sb.ToString();
            string result = await WebBrowser.CoreWebView2.ExecuteScriptAsync(cmd);

            Type resultType = typeof(TResult);
            return (TResult) JsonConvert.DeserializeObject(result, resultType);
        }

        /// <summary>
        /// Calls a method with simple parameters: String, number, boolean
        /// This version returns no results.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task Invoke(string method, params object[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(BaseInvocationTargetString + method + "(");

            if (parameters != null)
            {
                for (var index = 0; index < parameters.Length; index++)
                {
                    object parm = parameters[index];
                    var jsonParm = SerializeObject(parm);
                    sb.Append(jsonParm);
                    if (index < parameters.Length - 1)
                        sb.Append(",");
                }
            }
            sb.Append(")");

            await WebBrowser.CoreWebView2.ExecuteScriptAsync(sb.ToString());
        }

        /// <summary>
        /// Parameterizes a set of value parameters into string
        /// form that can be used in `ExecuteScriptAsync()` calls.
        /// Parameters are turned into a string using JSON values
        /// that are literal representations of values passed.
        ///
        /// You can wrap the result into a method call like this:
        ///
        /// ```csharp
        /// var parmData = js.Parameterize( new [] { 'parm1', pos } );
        /// "method(" + parmData + ")"
        /// ```
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public string Parameterize(object[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            if (parameters != null)
            {
                for (var index = 0; index < parameters.Length; index++)
                {
                    object parm = parameters[index];
                    var jsonParm = SerializeObject(parm);
                    sb.Append(jsonParm);
                    if (index < parameters.Length - 1)
                        sb.Append(",");
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Sets a property on the editor by name
        /// </summary>
        /// <param name="propertyName">Single property or hierarchical property off window.textEditor</param>
        /// <param name="value">Value to set - should be simple value</param>
        public async Task Set(string propertyName, object value)
        {

            var cmd = BaseInvocationTargetString + propertyName + " = " +
                      SerializeObject(value) + ";";

            await WebBrowser.CoreWebView2.ExecuteScriptAsync(cmd);
        }


        /// <summary>
        /// Gets a property from the window.textEditor object
        /// </summary>
        /// <param name="propertyName"></param>
        public async Task<TResult> Get<TResult>(string propertyName)
        {
            var cmd = "return " + BaseInvocationTargetString + propertyName + ";";
            string result = await WebBrowser.CoreWebView2.ExecuteScriptAsync(cmd);

            Type resultType = typeof(TResult);
            return DeserializeObject<TResult>(result);
        }

        /// <summary>
        /// Calls a method on the TextEditor in JavaScript a single JSON encoded
        /// value or object. The receiving function should expect a JSON object and parse it.
        ///
        /// This version returns no result value.
        /// </summary>
        public async Task CallMethodWithJson(string method, object parameter = null)
        {
            string cmd = method;

            if (parameter != null)
            {
                var jsonParm = SerializeObject(parameter);
                cmd += "(" + jsonParm + ")";
            }

            await WebBrowser.CoreWebView2.ExecuteScriptAsync(cmd);
        }

        /// <summary>
        /// Calls a method on the TextEditor in JavaScript a single JSON encoded
        /// value or object. The receiving function should expect a JSON object and parse it.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<TResult> CallMethodWithJson<TResult>(string method, object parameter = null)
        {
            string cmd = method;

            if (parameter != null)
            {
                var jsonParm = SerializeObject(parameter);
                cmd += "(" + jsonParm + ")";
            }

            string result = await WebBrowser.CoreWebView2.ExecuteScriptAsync(cmd);
            return DeserializeObject<TResult>(result);
        }

        /// <summary>
        /// Calls a method on the TextEditor in JavaScript a single JSON encoded
        /// value or object. The receiving function should expect a JSON object and parse it.
        /// </summary>
        public async Task ExecuteScriptAsync(string script)
        {
            await WebBrowser.CoreWebView2.ExecuteScriptAsync(script);
        }

        /// <summary>
        /// Calls a method on the TextEditor in JavaScript a single JSON encoded
        /// value or object.
        /// </summary>
        public async Task<TResult> ExecuteScriptAsyncWithResult<TResult>(string script)
        {
            var result = await WebBrowser.CoreWebView2.ExecuteScriptAsync(script);
            if (result == null)
                return default(TResult);

            return DeserializeObject<TResult>(result);
        }

        #endregion
    }

}
