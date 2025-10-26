using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Playnite.SDK.WebViewModels
{
    //
    // Summary:
    //     Resource type for a request.
    public enum ResourceType
    {
        //
        // Summary:
        //     Top level page.
        MainFrame = 0,
        //
        // Summary:
        //     Frame or iframe.
        SubFrame = 1,
        //
        // Summary:
        //     CSS stylesheet.
        Stylesheet = 2,
        //
        // Summary:
        //     External script.
        Script = 3,
        //
        // Summary:
        //     Image (jpg/gif/png/etc).
        Image = 4,
        //
        // Summary:
        //     Font.
        FontResource = 5,
        //
        // Summary:
        //     Some other subresource. This is the default type if the actual type is unknown.
        SubResource = 6,
        //
        // Summary:
        //     Object (or embed) tag for a plugin, or a resource that a plugin requested.
        Object = 7,
        //
        // Summary:
        //     Media resource.
        Media = 8,
        //
        // Summary:
        //     Main resource of a dedicated worker.
        Worker = 9,
        //
        // Summary:
        //     Main resource of a shared worker.
        SharedWorker = 10,
        //
        // Summary:
        //     Explicitly requested prefetch.
        Prefetch = 11,
        //
        // Summary:
        //     Favicon.
        Favicon = 12,
        //
        // Summary:
        //     XMLHttpRequest.
        Xhr = 13,
        //
        // Summary:
        //     A request for a ping
        Ping = 14,
        //
        // Summary:
        //     Main resource of a service worker.
        ServiceWorker = 15,
        //
        // Summary:
        //     A report of Content Security Policy violations.
        CspReport = 16,
        //
        // Summary:
        //     A resource that a plugin requested.
        PluginResource = 17,
        //
        // Summary:
        //     A main-frame service worker navigation preload request.
        NavigationPreLoadMainFrame = 19,
        //
        // Summary:
        //     A sub-frame service worker navigation preload request.
        NavigationPreLoadSubFrame = 20
    }

    public class Request
    {
        //
        // Summary:
        //     Request Method GET/POST etc
        public string Method { get; set; }
        //
        // Summary:
        //     Header Collection - If dealing with headers that only contain a single value
        //     then it's easier to use CefSharp.IRequest.SetHeaderByName(System.String,System.String,System.Boolean)
        //     or CefSharp.IRequest.GetHeaderByName(System.String). You cannot modify the referrer
        //     using headers, use CefSharp.IRequest.SetReferrer(System.String,CefSharp.ReferrerPolicy).
        //     NOTE: This collection is a copy of the underlying type, to make changes, take
        //     a reference to the collection, make your changes, then reassign the collection.
        public Dictionary<string, string> Headers { get; set; }
        //
        // Summary:
        //     Get the resource type for this request.
        public ResourceType ResourceType { get; set; }
        //
        // Summary:
        //     Get the referrer URL.
        public string ReferrerUrl { get; set; }
        //
        // Summary:
        //     Request Url
        public string Url { get; set; }
        //
        // Summary:
        //     Returns the globally unique identifier for this request or 0 if not specified.
        //     Can be used by CefSharp.IRequestHandler implementations in the browser process
        //     to track a single request across multiple callbacks.
        public ulong Identifier { get; }
    }

    //
    // Summary:
    //     Class used to represent a web response. The methods of this class may be called
    //     on any thread.
    public class Response
    {
        //
        // Summary:
        //     Get/Set the response charset.
        public string Charset { get; set; }
        //
        // Summary:
        //     MimeType
        public string MimeType { get; set; }
        //
        // Summary:
        //     Response Headers
        public Dictionary<string, string> Headers { get; set; }
        //
        // Summary:
        //     The status code of the response. Unless set, the default value used is 200 (corresponding
        //     to HTTP status OK).
        public int StatusCode { get; set; }
        //
        // Summary:
        //     Status Text
        public string StatusText { get; set; }
    }

    //
    // Summary:
    //     Flags that represent CefURLRequest status.
    public enum UrlRequestStatus
    {
        //
        // Summary:
        //     Unknown status.
        Unknown = 0,
        //
        // Summary:
        //     Request succeeded.
        Success = 1,
        //
        // Summary:
        //     An IO request is pending, and the caller will be informed when it is completed.
        IoPending = 2,
        //
        // Summary:
        //     Request was canceled programatically.
        Canceled = 3,
        //
        // Summary:
        //     Request failed for some reason.
        Failed = 4
    }
}
