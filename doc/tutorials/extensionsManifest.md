Extensions Manifest
=====================

Extension manifest files are used by Playnite to load extensions and display basic information to user on extension settings dialog. Manifest files are mandatory and extension won't be loaded unless valid manifest file is present inside extension directory under `extension.yaml` file name.

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

Examples
---------------------

**Example of script extension:**

```yaml
Name: Library Exporter
Author: Playnite
Version: 1.0
Module: LibraryExporter.ps1
Type: Script
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