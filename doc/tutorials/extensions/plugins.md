Plugins Introduction
=====================

Basics
---------------------

Plugins can be written in any .NET Framework compatible languages, this includes C#, VB.NET, F# and others, targeting .NET Framework 4.6.2.

Plugin types
---------------------

There are currently two types of plugins:

- `Generic plugins` Generic plugins offer same extensibility as scripts. You can add new entries to main menu or react to various [game events](events.md).

- `Library plugins`: Add ability to import games automatically as well as methods for metadata download for those games.

- `Metadata plugins`: Add ability to import game metadata.

Creating plugins
---------------------

> [!NOTE]
> Although you can technically write Playnite plugins in any IDE which supports .NET Framework 4.6.2 SDK, which includes [Rider](https://www.jetbrains.com/rider) and [Visual Studio Code](vscodeWindows.md), standard Visual Studio (including Community edition) is the preferred IDE, because it has the best .NET Framework support.

### 1. Generate plugin from template

Run [Toolbox](../toolbox.md) with arguments specific to a type of plugin you want to create.

For example, to create new library plugin:

```cmd
Toolbox.exe new LibraryPlugin "SomeLibrary importer" "d:\somefolder"
```

This will generate new C# project, with all of required classes already premade.

### 2. Implement functionality

Don't forget to implement functionality for template methods and properties that by default return `NotImplementedException` exception.

- `Generic plugins` - see generic plugins [documentation page](genericPlugins.md).
- `Library plugins` - see library plugins [documentation page](libraryPlugins.md).
- `Metadata plugins` - see metadata plugins [documentation page](metadataPlugins.md).

**Note to Visual Studio users:** If you are having issues compiling plugins created from the template, make sure that nuget dependencies are downloaded and installed properly. If you can't for some reason restore SDK nuget package, try following:

- Use "Save all" and save solution file if you are prompted to.
- Restart Visual Studio and open plugin solution file (.sln).
- Restore nuget dependencies via right-click menu on the solution item (in solution explorer, using "Restore NuGet packages").

### 3. Make Playnite load new extension

Go to Playnite settings, `For developers` section and add build output folder from the plugin project to `External extensions` list. This will look something like: `c:\plugin_project_folder\bin\Debug\` for debug builds by default. Build output folder can be seen/changed in VS project settings.

### 4. Debugging a plugin

Playnite plugins are standard .NET assemblies and therefore can be debugged in the same way as standard .NET dlls loaded in an external process, which is by default done via `Debug -> Attach to process` menu in Visual Studio. If you want to have an easier time and be able to start Playnite from VS directly with debugger attached to it automatically, do following:

- Open project properties via right-click menu on plugin project in solution explorer.
- Go to `Debug` section.
- Switch `Start action` to `Start external program` and set path to Playnite's executable.

Now when you start debugging, Visual Studio will automatically start Playnite process and attach debugger to it.

Accessing Playnite API
---------------------

Playnite API instance is available via [PlayniteAPI](xref:Playnite.SDK.Plugins.Plugin.PlayniteApi) property on your plugin class, the same instance which is also injected in plugin's constructor. In case you can't for some reason access this property, there's also static singleton instance accessible via [Playnite.SDK.API.Instance](xref:Playnite.SDK.API.Instance).

Plugin dependencies
---------------------

> [!WARNING] 
> If you are using external dependencies (from NuGet for example), make sure that you use the same version that Playnite already references. Current plugin system doesn't allow loading of multiple versions of the same assembly and you may encounter issues if you use different version compared to what Playnite uses.

You can check list of all Playnite's dependencies here:

- [Playnite](https://github.com/JosefNemec/Playnite/blob/master/source/Playnite/packages.config)
- [Playnite.DesktopApp](https://github.com/JosefNemec/Playnite/blob/master/source/Playnite.DesktopApp/packages.config)
- [Playnite.FullscreenApp](https://github.com/JosefNemec/Playnite/blob/master/source/Playnite.FullscreenApp/packages.config)

Probably the most common case where you might need to add an external dependency is for data serialization, usually JSON one. SDK already provides object serialization [methods](xref:Playnite.SDK.Data.Serialization) which should cover most serialization cases, including [DontSerialize](xref:Playnite.SDK.Data.DontSerializeAttribute) and [SerializationPropertyName](xref:Playnite.SDK.Data.SerializationPropertyNameAttribute) attributes.

> [!NOTE] 
> PlayniteSDK is designed in a way that all versions from one major version branch (for example 1.0, 1.1, 1.2 etc.) are backwards compatible. Therefore plugin written for SDK version 1.0 will also work with Playnite containing all 1.x versions of SDK. When loading plugins Playnite checks all SDK references and won't load plugins referencing incompatible SDK versions.

Referencing Playnite assemblies
---------------------

> [!WARNING] 
> **DO NOT** reference non-SDK Playnite assemblies in your project (`Playnite`, `Playnite.Common` etc.). Playnite will refuse to load plugins that reference those assemblies.

If you want to use functionality/code from non-SDK assemblies, you have several options:
* Open GitHub issues for the functionality to be exposed in the SDK.
* Link the source code to your project (choose "Add as link" when adding a source file into plugin project) and compile it with your plugin assembly.

Plugin settings
---------------------

If you want to give user ability to change plugin behavior, you can do that by implementing appropriate settings overrides from `Plugin` abstract class. Including ability to add fully customizable UI for your configuration that will be accessible in Playnite's settings windows. To add plugin settings support to your plugin follow [Plugin settings guide](pluginSettings.md).

Examples
---------------------

Support for all library integrations in Playnite is implemented via plugins, therefore built-in integrations (those offered during first time startup) can be used as reference examples. Source for "built-in" integrations can be found [on GitHub](https://github.com/JosefNemec/PlayniteExtensions).