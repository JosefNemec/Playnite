Script data directory
=====================

Introduction
---------------------

Scripts should store any generated data in a designated extension data directory instead of its installation directory, because installation directory gets purged during plugin installation and update.

To get script data directory, use `CurrentExtensionDataPath` global variable.

To get installation directory of currently running script, use `CurrentExtensionInstallPath` global variable.