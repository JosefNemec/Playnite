Emulator support
=====================

General import guide
---------------------

General workflow for importing games is:

- Setup an emulator via Library -> Configure Emulators menu.
  - Playnite can automatically import several well known emulators and will set all emulator properties for you. To import an emulator use `Import` button on configuration window.
  - See [Emulator support](#emulator-configuration) section for more details about emulator setup.  
- Import game image(s) (ROMs) via Add Game -> Emulated Game menu.
  - See [Game import support](#game-import-support) section for more details.  
  - If you want the same directory to be re-scanned on startup (or manually) enable "Save as auto-scan" option. You can later edit saved scanner from emulator configuration window, "Auto-scan configuration" tab.

> [!NOTE]
> It's highly recommended to use automatically imported emulators and built-in profiles for better game import experience. Some built-in emulators (for example RPCS3 and ScummVM) use more advanced import mechanism than just matching file names and you won't be able to utilize it with custom profiles.

> [!NOTE]
> Playnite currently doesn't have built-in support for any arcade emulator. You will have to configure those manually. Built-in support for more well known arcade emulators will be added [in future](https://github.com/JosefNemec/Playnite/issues/2407). If you are having issues with non-arcade emulators, please open GitHub issue (games not being imported or launched properly).

Emulator support
---------------------

Playnite has a support for handling and importing of emulated games. The support is implemented in two ways:

`Built-in support`: Playnite can recognize and import several known [emulators](https://github.com/JosefNemec/Playnite/tree/master/source/Playnite/Emulation/Emulators). This also includes import of emulated based on libretro game database.

If you think that there's an emulator that should be added to built-in list, please [open new issue on GitHub](https://github.com/JosefNemec/Playnite/issues/) for it to be added.

`Custom support`: You can manually configure any emulator that provides a way of launching specific games via command line arguments. Automatic game import might not be as accurate as in case of built-in profiles, since custom emulators only provide scanning based on a file name and file extension.

Game import support
---------------------

To import a game you need configure a scanner first. How games are imported is controller by an emulator and its selected profile. Built-in emulators/profiles use several different method how to detect a game.

Custom profiles primarily match games by specified file extensions. If you want to increase accuracy of the import, make sure you also assign correct platforms to the profile and that those platforms have [platform specification](libraryManager.md#platform-specification) assigned to them.

Playnite by default groups multi-disc games under one game entry. You can alternatively split or merge these via right-click menu after selecting games on import list. Right-click menu also gives you an ability to change platform and region in bulk.

Auto-scan configurations
---------------------

Scanner configurations can be used to automatically add new games during library import via `Update Game Library` menu. Specific configurations can be excluded from global library update on scanner configurations screen via `Library -> Configure Emulators` menu.

### Exclude patterns

These specify file patterns used during checksum scan. When a file matches specified pattern(s), its checksum won't be calculated and game will be imported based on other ROM properties (mainly file name). This can significantly speed up scanning process but also make import less accurate.

Multiple patterns can be specified by separating the list with comma, for example: `*.chd,*.iso`

> [!NOTE]
> `chd` files are excluded by default because there are currently no records for them in emulation database Playnite uses for game matching.


### Excluding files and folders from import completely

Scanner configurations allow to specify list of files and folders to be completely ignored during emulation scan. This list can be configured via `Exclusions` tab. Files and folders can be also added directly from scan results, via right-click menu on scanned games/files.

The list should contain relative file/folder paths, relative to scan folder specified in scanner's settings. For example, if you want to exclude `c:\test\dir\somefile.rom` file that is being detected by a scanner set to scan `c:\test\` folder, you would set exclusion to `dir\somefile.rom`. Or just `dir` to exclude the entire `dir` folder and its files from the scan.

### Exclude online files

Enabling this option will skip scan of files that are stored on cloud storage paths and are not currently downloaded, to prevent files from being downloaded during scanning process. Currently supported platforms are: Google Drive, DropBox and OneDrive.

### Relative path support

If `Import using relative paths` option is enabled, Playnite will try to import emulated games (paths to ROM files specifically) using relative paths. This works by replacing specific parts of the file path with `{PlayniteDir}` or `{EmulatorDir}` variables where possible if ROM location is inside emulator's or Playnite's folder.

Launching games
---------------------

If a game is imported via emulation import dialog, Playnite configures game launch via specific emulator (the one used to scan and import games) automatically. If you add a game manually, you can configure launch via an emulator by adding Emulator type [Play action](gameActions.md).

If a game has multiple ROM files assigned to it (on `Installation` tab while editing a game), Playnite will show selection during during game's startup, to specify which ROM should be passed to an emulator.

Emulator configuration
---------------------

Recommend way of adding new emulator is by importing it. If an emulator is recognized as support emulator, it will be show on import dialog with an option to select which emulator profiles to import. Some emulators support multiple profiles, which affect how a game is started and imported.

Alternatively you can add new emulator manually if built-in support doesn't work in a way you prefer, or you can just add custom profile to automatically imported built-in emulator.

### Emulator properties

| Property | Description |
| --- | --- |
| Installation Folder | Emulator's installation folder. Can be passed into profile configurations dynamically via `{EmulatorDir}` variable.
| Emulator specification | Built-in emulator specification used for an emulator. Specifies what built-in profiles can be added to an emulator.

### Profile properties 

Profiles handle how game is started and imported.

| Property | Description |
| --- | --- |
| Executable | File path to start an emulator. |
| Arguments | Startup arguments passed to an emulator during startup. |
| Working Directory | Working directory set to an emulator during startup. |
| Supported file types | File extensions separated by `,`. Used to detect ROM files by this profile. If you need to specify empty extension, use `<none>`. |
| Scripts | Profiles can execute custom scripts in the same way as [game or global scripts](gameScripts.md). |

### Startup script

If your profile contains `Startup script` code, Playnite will use that instead of general profile settings to launch an emulator. Emulator startup scripts works in the same way as [game startup scripts](gameScripts.md#startup-script). The only different is that emulator script have some additional variables available:

| Variable | Description |
| :-- | :-- |
| Emulator | [Emulator](xref:Playnite.SDK.Models.Emulator) selected to launch a game. |
| EmulatorProfile | [Emulator profile](xref:Playnite.SDK.Models.EmulatorProfile) selected to launch a game. |
| RomPath | ROM path selected to launch. |

### Custom profile example

Following example show how to configure `snex9x` emulator:
- Installation folder: `c:\some path\to\snes9x\`
- Executable: `{EmulatorDir}\snes9x.exe`
- Arguments: `"{ImagePath}" -fullscreen`
- Supported File Types: `zip, gz, jma, sfc, smc`

> [!NOTE]
> A lot of arcade emulators require ROM file to be passed via command line argument as a file name without complete path or file name without an extension. In that case you can use `{ImageName}` or `{ImageNameNoExt}` [game variables](gameVariables.md), instead of {ImagePath} which contains full path to a ROM file.

Troubleshooting
---------------------

If you encounter any issue when using built-in emulator configurations/profiles, please [open new issue on GitHub](https://github.com/JosefNemec/Playnite/issues/) to let us know and we will fix it.

### Game runs properly when launched manually from an emulator but not from Playnite

Playnite uses [command line arguments](https://www.bleepingcomputer.com/tutorials/understanding-command-line-arguments-and-how-to-use-them/) to tell specific emulator what game to start. But since some emulators can behave differently when a game is launched via command line compared to launched from emulator's UI, you can see games behaving differently when launched from Playnite.

This can be an issue in either Playnite or in an emulator:

- In case of an issue in Playnite, it means that Playnite is not passing correct arguments to an emulator, which usually happens when an emulator has been updated and requires different set of arguments (usually happens when using built-in emulator profiles) or if you set wrong arguments in your custom profile. You can test this by running an emulator manually from command prompt using the same command line arguments that Playnite uses (which can be seen on emulator config view). If the issue is in built-in emulator profile using wrong arguments, please open [new issue](https://github.com/JosefNemec/Playnite/issues/) in Playnite's repository for the profile to be updated.

- In case of an issue in an emulator, it means that Playnite is passing correct arguments, but there is a bug in the emulator which causes games to run badly when started from command line. The only solution for this is to contact emulator's developers and ask them to fix the issue in the emulator itself.

### Game run properly when launched manually from an emulator, but not from Playnite

Playnite uses [command line arguments](https://www.bleepingcomputer.com/tutorials/understanding-command-line-arguments-and-how-to-use-them/) to tell specific emulator what game to start. But since some emulators can behave differently when a game is launched via command line compared to launched from emulator's UI, you can see games behaving differently when launched from Playnite.

This can be an issue in either Playnite or in an emulator:

- In case of an issue in Playnite, it means that Playnite is not passing correct arguments to an emulator, which usually happens when an emulator has been updated and requires different set of arguments (usually happens when using built-in emulator profiles) or if you set wrong arguments in your custom profile. You can test this by running an emulator manually from command prompt using the same command line arguments that Playnite uses (which can be seen on emulator config view). If the issue is in built-in emulator profile using wrong arguments, please open [new issue](https://github.com/JosefNemec/Playnite/issues/) in Playnite's repository for the profile to be updated.

- In case of an issue in an emulator, it means that Playnite is passing correct arguments, but there is a bug in the emulator which causes games to run badly when started from command line. The only solution for this is to contact emulator's developers and ask them to fix the issue in the emulator itself.

### Emulator doesn't start

Usually occurs when using custom profile if executable path is not properly configured. Make sure that problematic profile points to an existing file.

### Emulator is not being imported

Playnite either doesn't have a profile for the emulator or the emulator has been updated in a way that prevents Playnite from detect it. In both case, please report an issue on GitHub

### Emulator starts, but game is not launched

This is usually caused by wrongly configured profile `Arguments` that are passed to the emulator. Make sure that arguments are configured according to what selected emulator supports (by checking its documentation).

### Game ROMs are not detected

When using custom emulator configurations, Playnite uses `Supported File Types` profile property to scan files ROM files. Make sure you have correct extensions specified.
