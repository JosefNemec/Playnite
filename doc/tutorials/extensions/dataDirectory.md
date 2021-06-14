Script data directory
=====================

Extensions should store any generated data in a designated extension data directory instead of its installation directory, because installation directory gets purged during extension installation or update.

Scripts
---------------------

To get script data directory, use `CurrentExtensionDataPath` global variable.

To get installation directory of currently running script, use `CurrentExtensionInstallPath` global variable.

Plugins
---------------------

To get the directory, call [GetPluginUserDataPath](xref:Playnite.SDK.Plugins.Plugin.GetPluginUserDataPath) method. The method returns full path designated to to a specific plugin. The same directory is used to store plugin [settings](pluginSettings.md).