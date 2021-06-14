### To get automatically notified about SDK changes, you can subscribe to [change tracking issue](https://github.com/JosefNemec/Playnite/issues/1425) on GitHub.

#### 5.5.0

* New
  * Ability to change progress text when using [ActivateGlobalProgress](xref:Playnite.SDK.IDialogsFactory)
  * `CurrentExtensionInstallPath` and `CurrentExtensionDataPath` global variables for script extensions
  
#### 5.4.0

* New
  * [ActivateGlobalProgress](xref:Playnite.SDK.IDialogsFactory) can now report specific progress status.
  * [CanExecuteJavascriptInMainFrame](xref:Playnite.SDK.IWebView) property for WebViews.
  * Various [data serialization methods](xref:Playnite.SDK.Data.Serialization) (JSON, YAML, TOML)
  * [CreateWindow](xref:Playnite.SDK.IDialogsFactory) for creating native Playnite windows with styling.
  * [GetCurrentAppWindow](xref:Playnite.SDK.IDialogsFactory) to get currently active Playnite window.

#### 5.3.0

* **Breaking Changes**:
  * Playnite will no longer load plugins that reference non-SDK Playnite assemblies. See [this page](tutorials/extensions/plugins.md#referencing-playnite-assemblies) for more information.
  * Playnite will no longer install extensions and themes that don't have proper version specified. The version string must a valid [.NET version string](https://docs.microsoft.com/en-us/dotnet/api/system.version)!

* **Now obsolete**:

  * These changes do not break compatibility in current version (mentioned methods are still available in SDK), but they will be made breaking in future major Playnite updates.
  * Added `Id` to extension and theme [manifests](tutorials/extensions/extensionsManifest.md). This field is currently not mandatory for existing extensions (Playnite 8 will load installed extensions without an ID, but will not install new ones without an ID), but should be provided for better extension installation and update support. Toolbox will not pack new extensions unless `Id` is present.
  * The way custom menu items are implemented (for main menu and game menu) has been completely changed (the old system still works temporarily). See [related documentation page](tutorials/extensions/menus.md) for more information.
  * [NavigationChanged](xref:Playnite.SDK.IWebView) from IWebView is now obsolete, use new `LoadingChanged` instead.

* New
  * Metadata plugins can now provide `Features`, `AgeRating`, `Series`, `Region` and `Platform` data.
  * Extensions can now provide [custom menu items](tutorials/extensions/menus.md) for game menus, including nested entries.
  * Most useful Playnite settings are now exposed in [IPlayniteSettingsAPI](xref:Playnite.SDK.IPlayniteSettingsAPI).
  * [ActivateGlobalProgress](xref:Playnite.SDK.IDialogsFactory) method to show blocking progress dialog.
  * [LoadingChanged](xref:Playnite.SDK.IWebView) event for WebViews.
  * [EvaluateScriptAsync](xref:Playnite.SDK.IWebView) method to execute JS code in a WebView.
  * [GetCookies](xref:Playnite.SDK.IWebView) method to get webview cookies.
  * [MarkdownToHtml](xref:Playnite.SDK.Data.Markup.MarkdownToHtml(System.String)) method for converting Markdown markup to HTML.

#### 5.2.0

* **Breaking Changes**:
  * [Toolbox](tutorials/toolbox.md) utility has been reworked and accepts different arguments then previously.

* New
  * Library plugins can now support extra [capabilities](tutorials/extensions/libraryPlugins.md#capabilities).
  * Added [ImportGame](xref:Playnite.SDK.IGameDatabase.ImportGame(Playnite.SDK.Models.GameInfo)) methods to more easily add new games to the library.
  * Added [OpenPluginSettings](xref:Playnite.SDK.IMainViewAPI.OpenPluginSettings(System.Guid)) method to open view with extension settings (also accessible via `OpenSettingsView` method inherited from `Plugin` class).
  * Added [StartGame](xref:Playnite.SDK.IPlayniteAPI.StartGame(System.Guid)).
  * Added [UriHandler](xref:Playnite.SDK.IPlayniteAPI.UriHandler) for registering of custom [URI method actions](tutorials/extensions/uriSupport.md).
  * Added option settings when creating offscreen web view (currently only option to disable JavaScript execution).
  * Added `OnGameSelected`, `OnApplicationStopped` and `OnLibraryUpdated` events.
  * Added `Features` game field and appropriate support for it in metadata plugins.
  * [Toolbox](tutorials/toolbox.md) utility can now generate plugins and scripts.
  * [Toolbox](tutorials/toolbox.md) utility can pack plugins and scripts into `.pext` file that can be used for easier distribution and installation.

#### 5.1.0

* New
  * Added support for creating metadata providers via [plugins](tutorials/extensions/metadataPlugins.md).
  * [ChooseImageFile](xref:Playnite.SDK.IDialogsFactory) method for dialogs API. **Only available in Desktop mode.**
  * [ChooseItemWithSearch](xref:Playnite.SDK.IDialogsFactory) method for dialogs API. **Only available in Desktop mode.**

#### 5.0.1

* Removed reference to LiteDB package. You can remove it from your plugin project if it's present.


#### 5.0.0

* **Breaking Changes**:
  * Extension plugins are no longer created by inheriting plugin interface, but rather extending [Plugin](xref:Playnite.SDK.Plugins.Plugin) and [LibraryPlugin](xref:Playnite.SDK.Plugins.LibraryPlugin) abstract classes.
  * [IGameDatabase](xref:Playnite.SDK.IGameDatabase) interface is completely changed and every object collection (Games, Genres, Tags etc.) is now accessible via appropriate `IItemCollection` property.
  * [Game](xref:Playnite.SDK.Models.Game) changed dramatically. Fields like genres, tags and others are no longer part of the model itself but just ID pointers to appropriate database objects.

* New
  * Extended several API with new methods.

#### 3.0.0

* **Breaking Changes**:
  * Removed and added new APIs and API members.
  * Game files are no longer stored in single database file. All game and media files are now accessible in their raw form even without user of database API.

#### 2.0.0

* **Breaking Changes**:
  * In order to unify terminology used in Playnite's UI and that in SDK, some classes and class members were renamed.
  * Extensions (both plugins and scripts) have to provide [extension manifest](tutorials/extensions/extensionsManifest.md) otherwise they won't be loaded.
    * Various information about extension (author, version etc.) must be now stored in manifest file.
  * Both plugins and scripts have to be stored in the same folder called `Extensions` (rather then in separate `Plugins` or `Scripts` folders).
  * Signature for default C# plugins has changed and they now have to implement `IGenericPlugin` interface to be loaded.

* New Plugin types. There are now two types of plugins that can be implemented:
  * Generic Plugin: Same as the old plugins.
  * [Library Plugin](tutorials/extensions/libraryPlugins.md): Used to add new library providers responsible for automatic game import from various sources.
    * All existing supported library importers (Steam, GOG etc.) are now distributed as [library plugins](https://github.com/JosefNemec/Playnite/tree/master/source/Plugins).

* New APIs:
  * Static [LogManager](xref:Playnite.SDK.LogManager) for easier log operations.
  * [Web Views API](xref:Playnite.SDK.IWebView) for creating web view windows or accessing offscreen browser.
  * [Resources API](xref:Playnite.SDK.IPlayniteAPI.Resources) for getting application resources like localized strings.
  * [Paths API](xref:Playnite.SDK.IPlayniteAPI.Paths) providing information about Playnite's application paths.
  * [Application Info API](xref:Playnite.SDK.IPlayniteAPI.ApplicationInfo) providing information about Playnite.
  
* New Methods
  * GetPluginUserDataPath: Gets path dedicated for plugins to store user data.
  * GetPluginConfiguration: Gets plugin configuration if available.
  * LoadPluginSettings: Loads plugin settings.
  * SavePluginSettings: Saves plugin settings.
  * ExpandGameVariables: Expands dynamic game variables in specified game action.
  * CreateLogger: Creates new instance of Playnite logger with name of calling class.

#### 1.1.0

* **Breaking Change**: Scripts and Plugins must be place in subfolders rather then directly inside of `Scripts` or `Plugins` folders.
* New: `OnGameStarting` event that will execute before game is started. See [events](tutorials/extensions/events.md) for use from scripts.
* New: [ShowErrorMessage](xref:Playnite.SDK.IDialogsFactory.ShowErrorMessage(System.String,System.String)) method in `IDialogsFactory`