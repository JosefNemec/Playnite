using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Events
{
    /// <summary>
    /// Represents loading event occuring in web view browser instance.
    /// </summary>
    public class WebViewLoadingChangedEventArgs
    {
        /// <summary>
        /// Gets or sets value indicating whether the page is loading.
        /// </summary>
        public bool IsLoading { get; set; }
    }

    /// <summary>
    /// Represents message from javascript.
    /// </summary>
    public class WebViewJavascriptMessageReceivedEventArgs
    {
        /// <summary>
        /// Gets or sets message from javascript
        /// </summary>
        /// 
        public string Message { get; set; }
    }

}
