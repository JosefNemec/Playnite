Extensions Manifest
=====================

Extension manifest files are used by Playnite to load extensions and display basic information to user on extension settings dialog. Manifest files are mandatory and extension won't be loaded unless valid manifest file is present inside  extension directory under `extension.yaml` file name.

Format
---------------------

Manifest is YAML formated file with following properties:

| Property | Description |
| -- | -- |
| Name | Extension name. |
| Author | Extension author. |
| Version | Extension version. |
| Module | File name of extension module: `*.dll` file for plugins, `*.ps1` or `*.py` file for scripts. |
| Type | Extension type, available values are: `Script`, `GenericPlugin`, `GameLibrary`, `MetadataProvider`. |
| Icon | Optional relative file name of extension icon. |
| Links | Optional list of links (extension website, changelog etc.) |

If you want to add executable script method into Playnite's main menu then `Functions` property has to be also specified. This only applies to script extensions since compiled plugins specify external functions directly.

Function properties:

| Property | Description |
| -- | -- |
| FunctionName | Name of the script function, function must not accept any parameters. |
| Description | Text description displayed on the exported menu entry. |

Examples
---------------------

**Example of script extension:**

```yaml
Name: Library Exporter
Author: Playnite
Version: 1.0
Module: LibraryExporter.ps1
Type: Script
Functions: 
    - Description: Export Library
      FunctionName: ExportLibrary
Links:
    - Name: Website
      Url: https://some.website.nowhere
```

**Example of library plugin:**

```yaml
Name: Some Library Plugin
Author: Playnite
Version: 1.0
Module: SomeLibraryPlugin.dll
Type: GameLibrary
Icon: pluginicon.png
Links:
    - Name: Website
      Url: https://some.website.nowhere
```