# Introduction to Playnite extensions

Basics
---------------------

Playnite can be extended via extensions implemented via scripts and plugins:

- Scripts: [PowerShell](https://docs.microsoft.com/en-us/powershell/) scripts are supported.
- Plugins: Any .NET Framework compatible language can be used (`C#`, `VB.NET`, `F#` and others).

Extensions fall under several categories of extended functionality that are available based on selected implementation:

| Functionality | Scripts | Plugins |
| -- | :--: | :--: |
| Adding game and main menu entries | • | • |
| Reacting to game events | • | • |
| Adding new UI elements |  | • |
| Injecting game actions |  | • |
| Library importer |  | • |
| Metadata provider |  | • |

- `Adding game and main menu entries` - ability to add new executable [menu entries](menus.md) to main menu and game menu.
- `Reacting to game events` - ability to execute code when various [game events](events.md) occur, like when game is started or stopped for example.
- `Adding new UI elements` - ability to add new [UI elements](ui.md) to various views and panels.
- `Injecting game actions` - gives ability to "inject" new Play, Install and Uninstall [game actions](gameActions.md) in real-time.
- `Library importer` - provides automatic import of games from various sources. For example all currently supported external clients (Steam, GOG, Origin etc.) [are implemented](https://github.com/JosefNemec/Playnite/tree/master/source/Plugins) via this extension type.
- `Metadata provider` - provides metadata for games in Playnite. Our default metadata provider, IGDB.com, is also [implemented as a metadata plugin](https://github.com/JosefNemec/Playnite/tree/master/source/Plugins/IGDBMetadata).

> [!WARNING] 
> Extension installation and update always replaces the entire extension directory completely. Meaning that any files that are not part of the installation package will be lost during installation process! It is highly recommended to store generated files in a separate extensions data folder. See [Data directories](dataDirectory.md) page to learn more about extension directories.

> [!NOTE]
> There's currently very active community around theme/extension development on our [Discord server](https://discord.gg/hSFvmN6). We highly recommend joining if you plan to develop add-ons for Playnite!

Creating Extensions
---------------------

It's highly recommended to use [Toolbox](../toolbox.md) utility to create new extensions. It will generate base directory structure and all files needed for you.

### 1. Directory structure and location

First create new extension folder inside of Playnite's `Extensions` directory. Location of `Extensions` directory differs based on Playnite's installation type:

- Portable version: `Extensions` folder directly inside of Playnite's installation location.
- Installed version: `%AppData%\Playnite\Extensions` folder.

> [!NOTE]
> You can load extensions from custom directories by adding them as developer plugins in Playnite's `For developers` settings menu.

### 2. Manifest file

Every extension must provide valid [manifest file](extensionsManifest.md) in order to be recognized and loaded by Playnite. Manifest is YAML formatted file called `extension.yaml` that must be stored inside of extension directory.

Resulting folder structure should look something like this:
```
├──Install directory or %AppData%\Playnite
│  └── Extensions
│      └── ExtensionFolder
│          ├── extension.yaml
│          └── scriptFileName.psm1 or pluginFileName.dll
```

See manifest file [documentation page](extensionsManifest.md) for more information about manifest contents.

### 3. Implementing extension

For scripts see [scripting introduction page](scripting.md).

For plugins see [plugins introduction page](plugins.md).

Loading extensions
---------------------

Extensions are loaded automatically by Playnite at every startup (unless extension is disabled via settings menu). Script can be reloaded at runtime via `Tools -> Reload Scripts` menu. Plugins can't be reloaded at runtime.

Distribution
---------------------

Use [Toolbox](../toolbox.md#packing-extensions) utility to package an extension or a theme and distribute `.pext` or `.pthm` file to users.

The best place to share extensions is via [Playnite add-on database](https://github.com/JosefNemec/PlayniteAddonDatabase), submitting an add-on there will make it available in Playnite's built-in add-on browser and will also enable easy add-on installation and updates.

It's also recommended to submit add-on entry to official Playnite forum, specifically [add-on database](https://playnite.link/forum/forum-3.html) sub-forum.