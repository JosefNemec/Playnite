Game actions
=====================

Introduction
---------------------

Game actions can be use either to start a game or to launch additional executables not related to game startup, for example configuration/mod utilities. Only actions marked as "Play" action are used to start a game, others are available to launch via game menu. If more then one Play action is available, Playnite will show selection dialog on game's startup to specify which action to use to start a game.

`Include library integration play actions` specifies whether integration plugin that imported a specific game, will be asked to handle game startup when the game is being launched.

Play actions can be also provided by plugins, see [game actions](../tutorials/extensions/gameActions.md) plugin page for more details.

Bulk editing
---------------------

When editing multiple games at the same time, Playnite won't show currently assigned actions from selected games. What's being assigned while in bulk edit mode will be assigned to all selected games and all existing actions will be removed.

Action properties
---------------------

| Property | Description |
| --- | --- |
| Play action | If enabled, Playnite will treat this action as Play action. Offering as an option when starting a game (if more than one option is available and counting play time from a started by this action. |
| Type | Action startup type. | 
| Tracking mode | Only available for Play actions (`File` and `URL` types) since it affects how play time detection works. |
| Path | File path (or URL) to start. |
| Arguments | Startup arguments passed to an executable during startup. |
| Working directory | Working directory set to an executable during startup. |

### Action types

| Property | Description |
| --- | --- |
| File | Path is executed as a standard executable file. |
| URL | Path is executed as an URL address. |
| Emulator | Action is started using emulator configuration. |
| Script | Script used to start an application. See [game scripts](gameScripts.md#startup-script) page for more details. |

> [!WARNING]
> Non-play actions that use `Script` startup method will run synchronously on main thead. This means that they will block Playnite's UI until the script is finished running. Therefore make sure you don't use any long running operations in your startup script.

### Emulator settings

| Property | Description |
| --- | --- |
| Emulator | Emulator to launch. |
| Emulator profile | Emulator profile to use to launch a game. Selecting `Choose on startup` will give an option to select a profile |

### Tracking mode

| Property | Description |
| --- | --- |
| Default | Playnite will try to detect and use the best tracking method automatically. |
| Process | Playnite will track a game as running as long as original process or any of its child processes are running. |
| Original process | Playnite will track a game as running as long as originally started process is running, child processes are ignored. |
| Folder | Playnite will track a game as running as long as some process from `Tracking path` folder is running. |

Troubleshooting
---------------------

In rare cases (depending on an application being started) the application won't start properly unless `Working directory` is not set to application's installation directory. If this happens you need to specify `Working directory` manually to a directory that makes selected application run properly. This is not an issue in Playnite, it's an issue in the started application.

### Using "Choose on startup" option doesn't show all emulators/profiles

This option uses platform field to select compatible emulators and profiles. If some emulators or profile are not shown on startup, make sure you have the same platforms assigned to a game and specific profiles.