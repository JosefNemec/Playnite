# Distributing Themes

Introduction
---------------------

You should only distribute files that your theme changes compared to the original source theme. It's because of the way theme loading is implemented in Playnite. Playnite is capable of loading only modified files while keeping everything else from original theme. This is to make sure that when change is made to original files, those changes are also applied to custom themes as well (unless custom theme provides their own version of that file).

This is done for a reason that theme files can contain functional elements (like buttons), that add or remove functionality and this makes sure that new functionality is also available in custom themes if possible.

If you distribute all files with your theme (even those you didn't modify) and update is made to Playnite that adds new functionality via theme files, your theme users won't be able to use that new functionality until you update the theme.

Manually created themes
---------------------

Pack theme files into a zip archive (without any root folder) and change file extension to `.pthm`. That way users will be able to [install it easily](installing.md).

Blend made themes
---------------------

If you used Blend to modify the theme then you need to use `Toolbox` utility to package your theme. Packaging via `Toolbox` makes sure that you only distribute files needed for your theme to work and nothing else.

To package theme run Toolbox with following arguments:

```
Toolbox.exe pack theme desktop <ThemeDirectoryName> <TargetFolder>
```

For example...

```
Toolbox.exe pack theme desktop "SuperClearModern" "c:\somedir"
```

...will create `c:\somedir\SuperClearModern.pthm` theme file you can distribute to users.

`<ThemeDirectoryName>` is name of the theme folder from `Themes\Desktop|Fullscreen` directory where you are developing the theme.

# Uploading themes

There's currently no official database with user made themes. We will be launching new forums soon (probably by the end July), which will be used as official theme database.

# Updating Themes

You will need to update themes from time to time to make sure they work with new Playnite versions properly.

Updates are necessary in these two cases:

* Theme API is updated with breaking changes (major version number changes, for example from `1.0.0` to `2.0.0`).
  * Playnite will not load themes that are not made for supported API version.
  * Minor updates will not break theme loading. For example Theme made for version `1.0.0` will still load in Playnite with API version `1.5.0`.

* New functionality is added to Playnite that requires update in theme file.
  * This usually means that your theme will still work (unless update means breaking change to Theme API), but users won't be able to make use of new features until the theme is updated.

You can follow changes to theme files by subscribing to [change tracking GitHub issue](https://github.com/JosefNemec/Playnite/issues/1259).

Blend made themes
---------------------

`Toolbox` utility can be used to automatically update theme files if you are developing themes in Blend.

**Updating via Toolbox utility is currently not available and will be possible in first Playnite 5.x update with theme changes.**