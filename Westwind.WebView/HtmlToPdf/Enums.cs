using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Westwind.WebView.HtmlToPdf
{
    public enum WebViewPrintColorModes
    {
        Color,
        Grayscale,
    }

    public enum WebViewPrintOrientations
    {
        Portrait,
        Landscape
    }

    public enum WebViewPrintCollations
    {
        Default,
        Collated,
        UnCollated
    }

    public enum WebViewPrintDuplexes
    {
        Default,
        OneSided,
        TwoSidedLongEdge,
        TwoSidedShortEdge

    }

}
