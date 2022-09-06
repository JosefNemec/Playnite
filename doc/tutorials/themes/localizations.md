Extension localizations
=====================

Info
---------------------

To add support for multiple languages:
- Create `Localization` directory in root of your extension.
- English is considered as base language, put all default strings into `en_US.xaml` file.
- Other languages must be stored in `{locale}.xaml` files. For example `cs_CZ.xaml` for Czech language.

List of currently supported languages can be seen [here](https://github.com/JosefNemec/Playnite/tree/master/source/Playnite/Localization). New languages and localizations can be contributed on our [Crowdin page](https://crowdin.com/project/playnite).

Localization files must contain proper `ResourceDictionary` objects, otherwise they won't be loaded. An example:

```xml
<?xml version="1.0"?>
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:sys="clr-namespace:System;assembly=mscorlib">
    <sys:String x:Key="LOCTestThemeSomeString">Test string</sys:String>
    <sys:String x:Key="LOCTestThemeMultilineString" xml:space="preserve">Test string
on multiple lines</sys:String>
</ResourceDictionary>
```

> [!WARNING]
> String keys are shared between all extensions so make sure you use unique keys for your extension if there's a possibility of any conflict. Current recommended practice is following format for string keys: `LOC<extension_name>KeyValue`, for example: `LOCTestThemeOpenSettings`

Referencing localized strings
---------------------

To reference localized string in XAML view, use `DynamicResource` markup.

```xml
<TextBlock Text="{DynamicResource stringkey}" />
```