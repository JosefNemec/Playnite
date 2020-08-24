Game variables
=====================

Following is the list of game variables that can be used in various places, specifically in:

- game links
- emulator configuration fields
- game action fields

To use a variable, encapsulate it with curly brackets in the string:

`some string {GameId} test`

|Variable|Description|
| ------------- | ------------- |
|InstallDir|Game installation directory|
|InstallDirName|Name of installation folder
|ImagePath|Game ISO/ROM path if set|
|ImageName|Game ISO/ROM file name|
|ImageNameNoExt|Game ISO/ROM file name without extension|
|PlayniteDir|Playnite's installation directory|
|Name|Game name |
|Platform|Game's platform |
|GameId|Game's ID |
|DatabaseId|Game's database ID |
|PluginId|Game's library plugin ID |
|Version|Game version|