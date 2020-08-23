Plugin data directory
=====================

Introduction
---------------------

Plugins should store any generated data in a designated extension data directory instead of its installation directory, because installation directory gets purged during plugin installation and update.

To get the directory, call [GetPluginUserDataPath](xref:Playnite.SDK.Plugins.Plugin.GetPluginUserDataPath) method. The method returns full path designated to to a specific plugin. The same directory is used to store plugin [settings](pluginSettings.md).