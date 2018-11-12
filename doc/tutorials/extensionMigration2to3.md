Extension Migration Guide 2.x > 3.x
=====================

Plugins
---------------------

- Create [extension manifest file](extensionsManifest.md) inside script extension folder.
- Update [SDK nuget](https://www.nuget.org/packages/PlayniteSDK/) to the latest version.
- Change your extension class from inheriting `Plugin` class to implementing [IGenericPlugin](xref:Playnite.SDK.Plugins.IGenericPlugin) interface.
- Update your extension with [renamed API objects](../memberChanges2_0.md).
- Don't forget to set proper `Module` attribute path inside extension manifest (pointing to extension dll file).
- Move plugin directory inside `Extensions` folder (previously `Plugins`)