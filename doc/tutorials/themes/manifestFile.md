# Theme manifest file

General information
---------------------

Every theme has to have manifest file name `theme.yaml`. This file is used by Playnite for several things including loading the theme and theme will not be usable in Playnite if manifest file is missing. The file should be in theme's root directory.

Available fields
---------------------

| Field | Description |
| -- | -- |
| Name | Theme's name that will be displayed during installation and on theme selection dialogs. |
| Author | Name of theme's author.  |
| Version  | Theme version. Any version string you want to assign to theme, not used by Playnite for anything specific right now. |
| Mode  | Specifies whether the theme is for Desktop of Fullscreen mode. |
| ThemeApiVersion | Theme API version required for theme to work. |
| Links | Optional list of links (extension website, changelog etc.) |


> [!WARNING] 
> Fields `Mode` and `ThemeApiVersion` are automatically generated and shouldn't be changed by hand.