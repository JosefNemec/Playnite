Playnite URI support
=====================

Introduction
---------------------

Plugins can register custom methods to be executed when specific `playnite://` URI is opened. This can be done using `RegisterSource` method from [UriHandler](xref:Playnite.SDK.IPlayniteAPI.UriHandler) API.

Example
---------------------

Following example executes method when `playnite://mysource/` URI is opened.

```csharp
PlayniteApi.UriHandler.RegisterSource("mysource", (args) =>
{
    // Code to be executed
    // Use args.Arguments to access URL arguments
});
```

In this example opening ``playnite://mysource/arg1/arg2` will call registered method and pass array of two arguments ("arg1" and "arg2") to it.