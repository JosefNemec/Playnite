Plugins Introduction
=====================

Basics
---------------------

Plugins can be written in any .NET Framework compatible languages, this includes C#, VB.NET, F# and others, targeting .NET Framework 4.6.

Plugins use the same Playnite API as [scripts](../scripts/scripting.md). All available interfaces and classes are exactly the same as is their use. For those reasons this plugin documentation mainly focuses on how to compile plugins and other differences compared to scripting. For specific API use cases see [scripts](../scripts/scripting.md) documentation sections.

Plugin types
---------------------

There are currently two types of plugins:

- `Generic plugins` Generic plugins offer same extensibility as scripts. You can add new entries to main menu or react to various [game events](../scripts/scriptingEvents.md).

- `Library plugins`: Add ability to import games automatically as well as methods for metadata download for those games.

Creating plugins
---------------------

Following examples will focus on use of C# and Visual Studio, but use with other languages is fairly similar.

### 1. Create plugin project

Start by creating new `Class Library` project targeting `.NET Framework 4.6.2`. Add [Playnite SDK](https://www.nuget.org/packages/PlayniteSDK/) nuget package reference and set reference to not require specific version (right-click on `Playnite.SDK` reference, choose `Properties` and set `Specific Version` to false).

> [!NOTE] 
> PlayniteSDK is designed in a way that all versions from one major version branch (for example 1.0, 1.1, 1.2 etc.) are backwards compatible. Therefore plugin written for SDK version 1.0 will also work with Playnite containing all 1.x versions of SDK. When loading plugins Playnite checks all SDK references and won't load plugins referencing incompatible SDK versions.

### 2. Write plugin

- `Generic plugins` - see generic plugins [documentation page](genericPlugins.md).
- `Library plugins` - see library plugins [documentation page](libraryPlugins.md).

### 3. Create manifest file

Described in [introduction section](../intro.md) to extensions.

Plugin settings
---------------------

If you want to provider user with an ability to change plugin behavior, you can do that by implementing appropriate properties from `ILibraryPlugin` interface. Including ability to add fully customizable UI for your configuration that will be accessible in Playnite's settings windows. To add plugin settings support to your plugin follow [Plugin settings guide](pluginSettings.md).

Examples
---------------------

Support for all 3rd part clients in Playnite is implemented fully using plugins so you can use then as a reference when implementing new ones. Source can be found [on GitHub](https://github.com/JosefNemec/Playnite/tree/master/source/Plugins).

Distributing plugins
---------------------

When distributing plugins it is ok to leave out dlls for `Json.Net` reference, since it's distributed with Playnite already.