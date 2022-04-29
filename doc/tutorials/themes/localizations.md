Extension localizations
=====================

Info
---------------------

To add support for multiple languages:
- Create `Localization` directory in root of your extension.
- English is considered as base language, put all default strings into `en_US.xaml` file.
- Other languages must be stored in `{locale}.xaml` files. For example `cs_CZ.xaml` for Czech language.

List of currently supported languages can be seen [here](https://github.com/JosefNemec/Playnite/tree/master/source/Playnite/Localization).

Localization files must contain proper `ResourceDictionary` objects, otherwise they won't be loaded. You can see examples of localization files [here](https://github.com/JosefNemec/Playnite/tree/master/source/Playnite/Localization).

> [!WARNING]
> String keys are shared between all extensions so make sure you use unique keys for your extension if there's a possibility of a conflict.

Referencing localized strings
---------------------

To reference localized string in XAML view, use `DynamicResource` markup.

```xml
<TextBlock Text="{DynamicResource stringkey}" />
```