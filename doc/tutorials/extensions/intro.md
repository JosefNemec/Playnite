# Introduction to Playnite extensions

Basics
---------------------

Playnite can be extended via extensions implemented via scripts and plugins:

- Scripts: [PowerShell](https://docs.microsoft.com/en-us/powershell/) and [IronPython](http://ironpython.net/) scripts are supported.
- Plugins: Any .NET Framework compatible language can be used (`C#`, `VB.NET`, `F#` and others).

Extensions fall under several categories of extended functionality that are available based on selected implementation:

| Extension | Scripts | Plugins |
| -- | :--: | :--: |
| Executable menu entry | • | • |
| Reacting to game events | • | • |
| Library importer |  | • |
| Metadata provider |  | • |

- `Executable menu entry` - adds new executable menu entry under main menu's `Scripts` sub sections.
- `Reacting to game events` - executes code when various game events occurs, like when game is started or stopped for example.
- `Library importer` - provides automatic import of games from various sources. For example all currently supported external clients (Steam, GOG, Origin etc.) [are implemented](https://github.com/JosefNemec/Playnite/tree/master/source/Plugins) via this extension type.
- `Metadata provider` - provides metadata for games in Playnite. Our default metadata provider, IGDB.com, is also [implemented as a metadata plugin](https://github.com/JosefNemec/Playnite/tree/master/source/Plugins/IGDBMetadata).

Creating Extensions
---------------------

It's highly recommended to use [Toolbox](../toolbox.md) utility to create new extensions. It will generate base directory structure and all files needed for you.

### 1. Directory structure and location

First create new extension folder inside of Playnite's `Extensions` directory. Location of `Extensions` directory differs based on Playnite's installation type:

- Portable version: `Extensions` folder directly inside of Playnite's installation location.
- Installed version: `%AppData%\Playnite\Extensions` folder.

> [!WARNING] 
> Automatic installations and updates via `.pext` and `.pthm` files always replace the extension/theme directory completely. Meaning that any files that are not part of the installation package will be lost during installation process! It is highly recommended to store generated files in a separate extensions data folder. See [Data directories](dataDirectory.md) page to learn more about extension directories.

### 2. Manifest file

Every extension must provide valid [manifest file](extensionsManifest.md) in order to be recognized and loaded by Playnite. Manifest is YAML formatted file called `extension.yaml` that must be stored inside of extension directory.

Resulting folder structure should look something like this:
```
├──Install directory or %AppData%\Playnite
│  └── Extensions
│      └── PluginFolder
│          ├── extension.yaml
│          └── scriptFileName.py or pluginFileName.dll
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

The best place to share extensions is official Playnite forum, specifically [extension database](https://playnite.link/forum/forum-3.html) sub-forum.