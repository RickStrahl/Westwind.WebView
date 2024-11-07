using Microsoft.Web.WebView2.Core;

namespace Westwind.WebView.HtmlToPdf
{

    /// <summary>
    /// Proxy object of Core WebView settings options to avoid requiring
    /// a direct reference to the WebView control in the calling
    /// application/project.
    /// 
    /// Settings map to these specific settings in the WebView:
    /// https://chromedevtools.github.io/devtools-protocol/tot/Page/#method-printToPDF
    /// </summary>
    public class WebViewPrintSettings
    {       
        /// <summary>
        /// Scale Factor up to 2
        /// </summary>
        public float ScaleFactor
        {
            get => _scaleFactor;
            set
            {
                _scaleFactor = value;
                if (_scaleFactor > 2F)
                    ScaleFactor = 2F;
            }
        }
        private float _scaleFactor = 1F;


        /// <summary>
        /// Portrait, Landscape
        /// </summary>
        public WebViewPrintOrientations Orientation { get; set; } = WebViewPrintOrientations.Portrait;

        /// <summary>
        /// Width in inches
        /// </summary>
        public double PageWidth { get; set; } = 8.5F;

        /// <summary>
        /// Height in inches
        /// </summary>
        public double PageHeight { get; set; } = 11F;


        /// <summary>
        /// Top Margin in inches
        /// </summary>
        public double MarginTop { get; set; } = 0.4F;

        /// <summary>
        /// Bottom Margin in inches
        /// </summary>
        public double MarginBottom { get; set; } = 0.30F;

        /// <summary>
        /// Left Margin in inches
        /// </summary>
        public double MarginLeft { get; set; } = 0.25F;

        /// <summary>
        /// Right Margin in inches
        /// </summary>
        public double MarginRight { get; set; } = 0.25F;


        /// <summary>
        /// Page ranges as specified 1,2,3,5-7
        /// </summary>
        public string PageRanges { get; set; } = string.Empty;


        /// <summary>
        /// Determines whether background colors are printed. Use to
        /// save ink on printing or for more legible in print/pdf scenarios
        /// </summary>
        public bool ShouldPrintBackgrounds { get; set; } = true;


        /// <summary>
        /// Color, Grayscale, Monochrome
        /// 
        /// CURRENTLY DOESN'T WORK FOR PDF GENERATION
        /// </summary>
        public WebViewPrintColorModes ColorMode { get; set; } = WebViewPrintColorModes.Color;


        /// <summary>
        /// When true prints only the section of the document selected 
        /// </summary>
        public bool ShouldPrintSelectionOnly { get; set; } = false;

        /// <summary>
        /// Determines whether headers and footers are printed
        /// </summary>
        public bool ShouldPrintHeaderAndFooter { get; set; } = false;

        
        public bool GenerateDocumentOutline { get; set; } = true;


        /// <summary>
        /// Html Template that renders the header.
        /// Refer to for embeddable styles and formatting:
        /// https://chromedevtools.github.io/devtools-protocol/tot/Page/#method-printToPDF
        /// </summary>
        public string HeaderTemplate { get; set; } = "<div style='font-size: 11.5px; width: 100%; text-align: center;'><span class='title'></span></div>";


        /// <summary>
        /// Html template that renders the footer
        /// Refer to for embeddable styles and formatting:
        /// https://chromedevtools.github.io/devtools-protocol/tot/Page/#method-printToPDF
        /// </summary>
        public string FooterTemplate { get; set; } = "<div style='font-size: 10px; clear: all; width: 100%; margin-right: 3em; text-align: right; '><span class='pageNumber'></span> of <span class='totalPages'></span></div>";



        /// <summary>
        /// This a shortcut for the HeaderTemplate that sets the top of the page header. For more control
        /// set the HeaderTemplate directly.
        /// </summary>
        public string HeaderTitle { set
            {
                if (string.IsNullOrEmpty(value))
                    HeaderTemplate = "";
                else
                    HeaderTemplate = $"<div style='font-size: 11.5px; width: 100%; text-align: center;'>{value}</div>";
            }
        }

        /// <summary>
        /// This a shortcut for the FooterTemplate that sets the bottom of the page footer. For more control
        /// set the FooterTemplate directly.        
        /// </summary>
        public string FooterText
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    FooterTemplate = "";
                else
                    FooterTemplate = $"<div style='font-size: 10px; margin-right: 2em; width: 100%; text-align: right; '>{value}</div>";
            }
        }

        #region Print Settings - ignored for PDF

        /// <summary>
        /// Printer name when printing to a printer (not applicable for PDF)
        /// 
        /// NO EFFECT ON PDF PRINTING
        /// </summary>
        public string PrinterName { get; set; }

        /// <summary>
        /// Number of Copies to print
        /// 
        /// NO EFFECT ON PDF PRINTING
        /// </summary>
        public int Copies { get; set; } = 1;

        /// <summary>
        /// Default, OneSided, TwoSidedLongEdge, TwoSidedShortEdge
        /// 
        /// NO EFFECT ON PDF PRINTING
        /// </summary>
        public WebViewPrintDuplexes Duplex { get; set; } = WebViewPrintDuplexes.Default;

        /// <summary>
        /// Default, Collated, Uncollated
        /// 
        /// NO EFFECT OF PDF PRINTING
        /// </summary>
        public WebViewPrintCollations Collation { get; set; } = WebViewPrintCollations.Default;

        /// <summary>
        /// Allows multiple pages to be packed into a single page.
        /// 
        /// NO EFFECT ON PDF PRINTING
        /// </summary>
        public int PagesPerSide { get; set; } = 1;

        #endregion
    }
}