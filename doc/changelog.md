#### 5.0.0

* **Breaking Changes**:
  * Extension plugins are no longer created by inheriting plugin interface, but rather extending [Plugin](xref:Playnite.SDK.Plugins.Plugin) and [LibraryPlugin](xref:Playnite.SDK.Plugins.LibraryPlugin) abstract classes.
  * [IGameDatabase](xref:Playnite.SDK.IGameDatabase) interface is completely changed and every object collection (Games, Genres, Tags etc.) is now accessible via appropriate `IItemCollection` property.
  * [Game](xref:Playnite.SDK.Models.Game) changed dramatically. Fields like genres, tags and others are no longer part of the model itself but just ID pointers to appropriate database objects.

* New
  * Extended several API with new methods.

#### 3.0.0

* **Breaking Changes**:
  * Removed and added new APIs and API members, see [full list](memberChanges3_0.md) of changes.
  * Game files are no longer stored in single database file. All game and media files are now accessible in their raw form even without user of database API.
  * See [migration tutorial](tutorials/extensionMigration2to3.md) for step by step guide on how update existing extensions.

#### 2.0.0

* **Breaking Changes**:
  * In order to unify terminology used in Playnite's UI and that in SDK, some classes and class members [were renamed](memberChanges2_0.md).
  * Extensions (both plugins and scripts) have to provide [extension manifest](tutorials/extensionsManifest.md) otherwise they won't be loaded.
    * Various information about extension (author, version etc.) must be now stored in manifest file.
  * Both plugins and scripts have to be stored in the same folder called `Extensions` (rather then in separate `Plugins` or `Scripts` folders).
  * Signature for default C# plugins has changed and they now have to implement `IGenericPlugin` interface to be loaded.

* See [migration tutorial](tutorials/extensionMigration1to2.md) for step by step guide on how to convert existing extensions to a new format.

* New Plugin types. There are now two types of plugins that can be implemented:
  * Generic Plugin: Same as the old plugins.
  * [Library Plugin](tutorials/plugins/libraryPlugins.md): Used to add new library providers responsible for automatic game import from various sources.
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
* New: [OnGameStarting](xref:Playnite.SDK.Plugins.IGenericPlugin.OnGameStarting(Playnite.SDK.Models.Game)) event that will execute before game is started. See [events](tutorials/scripts/scriptingEvents.md) for use from scripts.
* New: [ShowErrorMessage](xref:Playnite.SDK.IDialogsFactory.ShowErrorMessage(System.String,System.String)) method in `IDialogsFactory`