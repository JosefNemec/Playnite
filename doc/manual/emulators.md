Emulator support
=====================

Introduction
---------------------

Playnite has a support for handling and importing of emulated games. The support is implemented in two ways:

`Built-in support`: Playnite can recognize and import several known [emulators](https://github.com/JosefNemec/Playnite/tree/master/source/Playnite/Emulation/Emulators). This also includes import of emulated based on libretro game database.

If you think that there's an emulator that should be added to built-in list, please [open new issue on GitHub](https://github.com/JosefNemec/Playnite/issues/) for it to be added.

`Custom support`: You can manually configure any emulator that provides a way of launching specific games via command line arguments. Automatic game import might not be as accurate as in case of built-in profiles, since custom emulators only provide scanning based on a file name and file extension.

> [!NOTE]
> Playnite currently doesn't have built-in support for any arcade emulator. You will have to configure those manually. Built-in support for more well known arcade emulators will be added [in future](https://github.com/JosefNemec/Playnite/issues/2407). Also please open issue if built-in support for a specific emulator doesn't work as expected (games not being imported or launched properly).

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
| Emulator | Emulator selected to launch a game. |
| EmulatorProfile | Emulator profile selected to launch a game. |
| RomPath | ROM path selected to launch. |

### Custom profile example

Following example show how to configure `snex9x` emulator:
- Installation folder: `c:\some path\to\snes9x\`
- Executable: `{EmulatorDir}\snes9x.exe`
- Arguments: `"{ImagePath}" -fullscreen`
- Supported File Types: `zip, gz, jma, sfc, smc`

> [!NOTE]
> A lot of arcade emulators require ROM file to be passed via command line argument as a file name without complete path or file name without an extension. In that case you can use `{ImageName}` or `{ImageNameNoExt}` [game variables](gameVariables.md), instead of {ImagePath} which contains full path to a ROM file.

Game import support
---------------------

To import a game you need configure a scanner first. How games are imported is controller by an emulator and its selected profile. Built-in emulators/profiles use several different method how to detect a game.

Custom profiles primarily match games by specified file extensions. If you want to increase accuracy of the import, make sure you also assign correct platforms to the profile and that those platforms have [platform specification](libraryManager.md#platform-specification) assigned to them.

Playnite by default groups multi-disc games under one game entry. You can alternatively split or merge these via right-click menu after selecting games on import list. Right-click menu also gives you an ability to change platform and region in bulk.

Auto-scan configurations
---------------------

Scanner configurations can be used to automatically add new games during library import via `Update Game Library` menu. Specific configurations can be excluded from global library update on scanner configurations screen via `Library -> Configure Emulators` menu.

Launching games
---------------------

If a game is imported via emulation import dialog, Playnite configures game launch via specific emulator (the one used to scan and import games) automatically. If you add a game manually, you can configure launch via an emulator by adding Emulator type [Play action](gameActions.md).

If a game has multiple ROM files assigned to it (on `Installation` tab while editing a game), Playnite will show selection during during game's startup, to specify which ROM should be passed to an emulator.

Troubleshooting
---------------------

If you encounter any issue when using built-in emulator configurations/profiles, please [open new issue on GitHub](https://github.com/JosefNemec/Playnite/issues/) to let us know and we will fix it.

### Emulator is not being imported

Playnite either doesn't have a profile for the emulator or the emulator has been updated in a way that prevents Playnite from detect it. In both case, please report an issue on GitHub

### Emulator starts, but game is not launched

This is usually caused by wrongly configured profile `Arguments` that are passed to the emulator. Make sure that arguments are configured according to what selected emulator supports (by checking its documentation).

### Game ROMs are not detected

When using custom emulator configurations, Playnite uses `Supported File Types` profile property to scan files ROM files. Make sure you have correct extensions specified.