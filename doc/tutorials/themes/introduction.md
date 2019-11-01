# Introduction to Themes

General information
---------------------

Playnite's user interface is implemented using Windows Presentation Framework (WPF) and UI definition is done using [XAML](https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/xaml-overview-wpf) files. Custom themes in Playnite are implemented using [standard template and styling](https://docs.microsoft.com/en-us/dotnet/framework/wpf/controls/styling-and-templating) support that WPF provides, therefore any tutorial that applies to styling in WPF also applies to Playnite.

Fullscreen and Desktop modes
---------------------

Playnite offers two separate modes of operation. Standard `Desktop` mode designed for keyboard and mouse and `Fullscreen` mode designed to be controlled with gamepad. These two modes are implemented completely separately and therefore themes are also completely separate.

Learning resources
---------------------

Since Playnite themes are essentially just set of [template and style](https://docs.microsoft.com/en-us/dotnet/framework/wpf/controls/styling-and-templating) files, general editing rules and tutorials that apply to WPF also apply to Playnite.

Recommended WPF resources:
* https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/xaml-overview-wpf
* https://www.wpftutorial.net/GettingStarted.html
* https://www.tutorialspoint.com/wpf/

Creating Playnite themes
---------------------

> [!WARNING] 
> Do not edit built-in default themes. Always create new copy of theme files are edit those. Broken edits to default theme files could to Playnite not being able to start anymore.

There are generally two approaches to theme creation in Playnite.

1. **[Manually editing](manualEditing.md)** XAML files using any text editor.

2. **[Using Blend/Visual Studio](usingDesigner.md)** designer.

Option #1 doesn't require installation of any additional applications and themes can be generally created even using Notepad. However this approach has major disadvantages:
* You don't get live preview of changes your are making.
* You have to restart Playnite every time a change is made to theme files.
* There's not autocompletion or error checking for XAML syntax.

Option #2 requires installation of [Visual Studio IDE](https://visualstudio.microsoft.com/), Community edition is free to download and includes [Blend](https://docs.microsoft.com/en-us/visualstudio/designers/creating-a-ui-by-using-blend-for-visual-studio?view=vs-2019) editor. This approach takes some time to set up, but offers all advantages that manual editing lacks, like live preview, autocompletion of XAML properties, visual editor etc. 

**Using Blend editor is recommended option.**

Theme files and directories
---------------------

This section explains contents and purpose of specific theme files.

| Directory/File | Description |
| -- | -- |
| DefaultControls | Styles for standard (built-in WPF) controls like button, checkbox etc. |
| DerivedStyles | Styles for standard (built-in WPF) controls like button, checkbox etc., that are used in specific cases. For example Play button, list item for Grid view etc. |
| CustomControls | Styles for custom Playnite controls. |
| Views | Styles for library views and panels. |
| Common.xaml | Base styles that are inherited by other styles from the theme. |
| Constants.xaml | Colors, brushes, sizes and other constants used by styles form the theme. |
| Media.xaml | Various icons, texts and image specifications used by styles form the theme.  |