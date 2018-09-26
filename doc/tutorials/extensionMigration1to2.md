Extension Migration Guide 1.x > 2.x
=====================

Scripts
---------------------

- Create [extension manifest file](extensionsManifest.md) inside script extension folder.
- Move definition of `__attributes` and `__exports` variables into the manifest file.
- Don't forget to set proper `Module` attribute path inside extension manifest (pointing to script file).
- Update your script with [renamed API objects](../memberChanges2_0.md).
- Move script directory inside `Extensions` folder (previously `Scripts\<script_type>\`)

Plugins
---------------------

- Create [extension manifest file](extensionsManifest.md) inside script extension folder.
- Update [SDK nuget](https://www.nuget.org/packages/PlayniteSDK/) to the latest version.
- Change your extension class from inheriting `Plugin` class to implementing [IGenericPlugin](xref:Playnite.SDK.Plugins.IGenericPlugin) interface.
- Update your extension with [renamed API objects](../memberChanges2_0.md).
- Don't forget to set proper `Module` attribute path inside extension manifest (pointing to extension dll file).
- Move plugin directory inside `Extensions` folder (previously `Plugins`)