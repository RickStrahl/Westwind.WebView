using System;
using System.IO;

namespace Westwind.WebView.HtmlToPdf
{

    /// <summary>
    /// Result from a Print to PDF operation. ResultStream is set only
    /// on stream operations.
    /// </summary>
    public class PdfPrintResult
    {
        /// <summary>
        /// Notifies of sucess or failure of operation
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// If in stream mode, the resulting MemoryStream will be assigned
        /// to this property. You need to close/dispose of this stream when
        /// done with it.
        /// </summary>
        public Stream ResultStream { get; set; }

        /// <summary>
        /// A message related to the operation - use for error messages if
        /// an error occured.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The exception that triggered a failed PDF conversion operation
        /// </summary>
        public Exception LastException { get; set; }
    }
}