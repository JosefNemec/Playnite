Web views
=====================

Introduction
---------------------

If you need to access specific web resource using a "real" browser, you can use web view API to create Chromium (specifically [CEF](https://bitbucket.org/chromiumembedded/cef)) browser instance. For example most library integration plugins use this to handle account authentication, to show web sign-in form.

Creating web views
---------------------

Use [WebViews](xref:Playnite.SDK.IPlayniteAPI.WebViews) API to create new web views. There are currently two types of views that can be created: normal and offscreen. Normal is a webview that opens new window with website content. Offscreen creates new browser instance without any window or visual feedback. Offscreen instances are useful if you need to access some web resource but you don't need any user interaction or have the data actually visible to the user from the browser window.

Notes
---------------------

Web view data are isolated for each extension. This means that cache, cookies and other web view data are not shared between extensions and separate instance is kept for each extension individually. This also means that methods like cookie deletion also only affect specific extension which calls those methods.

Use can use `F12` key to open standard Chromium developer tools while normal web view window is opened.

As already noted, almost all integration plugins use web views in some form so they are good source of inspiration if you need some practical examples of how to use web view API.