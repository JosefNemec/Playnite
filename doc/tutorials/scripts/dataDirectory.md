Script data directory
=====================

Introduction
---------------------

Scripts should store any generated data in a designated extension data directory instead of its installation directory, because installation directory gets purged during plugin installation and update.

To get script data directory, use [ExtensionsDataPath](xref:Playnite.SDK.IPlaynitePathsAPI.ExtensionsDataPath) property from [Paths](xref:Playnite.SDK.IPlayniteAPI#Playnite_SDK_IPlayniteAPI_Paths) API. The property returns root directory for all extensions, you should create and use sub-directory specific to your script extension, in order to avoid possible conflicts with other script extensions.

