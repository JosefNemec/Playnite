# Theme manifest file

General information
---------------------

Every theme has to have manifest file name `theme.yaml`. This file is used by Playnite for several things including loading the theme and theme will not be usable in Playnite if manifest file is missing. The file should be in theme's root directory.

Available fields
---------------------

| Field | Description |
| -- | -- |
| Id | Unique string identifier for the theme. Must not be shared with any other theme. |
| Name | Theme's name that will be displayed during installation and on theme selection dialogs. |
| Author | Name of theme's author.  |
| Version | Extension version, must be a valid [.NET version string](https://docs.microsoft.com/en-us/dotnet/api/system.version). |
| Mode  | Specifies whether the theme is for Desktop of Fullscreen mode. |
| ThemeApiVersion | Theme API version required for theme to work. |
| Links | Optional list of links (extension website, changelog etc.) |


> [!WARNING] 
> Fields `Mode` and `ThemeApiVersion` are automatically generated and shouldn't be changed by hand, unless you are updating the theme to support newer versions of Playnite.