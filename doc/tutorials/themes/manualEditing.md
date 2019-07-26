# Creating themes manually

Basics
---------------------

This method is generally not recommended and using [Blend](usingDesigner.md) is recommended method. However for smaller changes this is usable method.

Creating theme
---------------------

* Create empty folder inside of `Themes` directory (and `Fullscreen` or `Desktop` subdirectory).
* Create theme [manifest file](manifestFile.md).
    * Set proper `Mode` and `ThemeApiVersion` fields. `ThemeApiVersion` of currently installed Playnite version can be found by opening `About Playnite` menu from Desktop mode.
* Copy original theme file (xaml, image etc.) to the new folder (make sure you keep the directory structure).
* Make changes to copied file using any text editor.

Testing changes
---------------------
 
To test theme in Playnite itself, just start Playnite and change theme selection in the application settings. No additional steps should be needed for Playnite to load the theme as long as theme manifest is present.

Packaging theme for distribution
---------------------

See [Distribution and Updates](distributionAndUpdates.md) page for more details.