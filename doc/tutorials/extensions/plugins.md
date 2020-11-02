Plugins Introduction
=====================

Basics
---------------------

Plugins can be written in any .NET Framework compatible languages, this includes C#, VB.NET, F# and others, targeting .NET Framework 4.6.

Plugin types
---------------------

There are currently two types of plugins:

- `Generic plugins` Generic plugins offer same extensibility as scripts. You can add new entries to main menu or react to various [game events](events.md).

- `Library plugins`: Add ability to import games automatically as well as methods for metadata download for those games.

- `Metadata plugins`: Add ability to import game metadata.

Creating plugins
---------------------

### Using Toolbox

#### 1. Generate from template

Run [Toolbox](../toolbox.md) with arguments specific to a type of plugin you want to create.

For example, to create new library plugin:

```
Toolbox.exe new LibraryPlugin "SomeLibrary importer" "d:\somefolder"
```

This will generate new C# project, with all of required classes already premade.

#### 2. Implement functionality

Don't forget to implement functionality for template methods and properties that by default return `NotImplementedException` exception.

> [!NOTE] 
If you are having issue compiling plugin created from template, then make sure that nuget dependencies are downloaded and installed properly. You can do that by using "Manage NuGet Packages" menu after right-clicking on plugin solution/project in solution explorer.

### Manually

#### 1. Create plugin project

Start by creating new `Class Library` project targeting `.NET Framework 4.6.2`. Add [Playnite SDK](https://www.nuget.org/packages/PlayniteSDK/) nuget package reference and set reference to not require specific version (right-click on `Playnite.SDK` reference, choose `Properties` and set `Specific Version` to false).

> [!NOTE] 
> PlayniteSDK is designed in a way that all versions from one major version branch (for example 1.0, 1.1, 1.2 etc.) are backwards compatible. Therefore plugin written for SDK version 1.0 will also work with Playnite containing all 1.x versions of SDK. When loading plugins Playnite checks all SDK references and won't load plugins referencing incompatible SDK versions.

#### 2. Write a plugin

- `Generic plugins` - see generic plugins [documentation page](genericPlugins.md).
- `Library plugins` - see library plugins [documentation page](libraryPlugins.md).
- `Metadata plugins` - see metadata plugins [documentation page](metadataPlugins.md).

#### 3. Create manifest file

Described in [introduction section](../intro.md) to extensions.

Plugin dependencies
---------------------

> [!WARNING] 
> If you are using external dependencies (from NuGet for example), make sure that you use the same version that Playnite already references. Current plugin system doesn't allow loading of multiple versions of the same assembly and you may encounter issues if you use different version compared to what Playnite uses.

You can check list of all Playnite's dependencies here:

- [Playnite](https://github.com/JosefNemec/Playnite/blob/master/source/Playnite/packages.config)
- [Playnite.Common](https://github.com/JosefNemec/Playnite/blob/master/source/Playnite.Common/packages.config)
- [Playnite.DesktopApp](https://github.com/JosefNemec/Playnite/blob/master/source/Playnite.DesktopApp/packages.config)
- [Playnite.FullscreenApp](https://github.com/JosefNemec/Playnite/blob/master/source/Playnite.FullscreenApp/packages.config)

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

Support for all 3rd part clients in Playnite is implemented fully using plugins so you can use then as a reference when implementing new ones. Source can be found [on GitHub](https://github.com/JosefNemec/Playnite/tree/master/source/Plugins).

Distributing plugins
---------------------

It's highly recommend to use [Toolbox](../toolbox.md) to package the plugin and distribute resulting `.pext` file. Packaging removes unnecessary references automatically and makes it easier for users to install your extension.

When distributing plugins it is ok to leave out dlls for `Json.Net` reference, since it's distributed with Playnite already.