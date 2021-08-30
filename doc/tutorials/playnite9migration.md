Playnite 9 add-on migration guide
=====================


Extension load changes
---------------------


### Scripts

**IronPython support has been removed completely, therefore you have to rewrite those to PowerShell.**

PowerShell extensions are now imported as [PowerShell modules](https://docs.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-script-module?view=powershell-5.1). The extension of the file must be `.psm1` (or `.psd1` if you use a [PowerShell module manifest](https://docs.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-module-manifest?view=powershell-5.1)).

Any exported functions from your extension must be exported from the module. In a `.psm1` file all functions in the module scope are exported by default, but functions in the global scope (defined like `function global:OnGameStarted()`) will _not_ be correctly exported. Exported functions must accept the *exact* number of arguments that Playnite passes to them.

### Plugins

Generic plugins now have to inherit from [GenericPlugin](xref:Playnite.SDK.Plugins.GenericPlugin) class, not `Plugin` class.

Model changes
---------------------

Multiple changes have been made to various data models, properties removed, renamed, addded.

### Game

| Old | New | Notes |
| :-- | :-- | :-- |
|  |  |  |


> [!NOTE] 
> Since PowerShell is not statically typed language there's no easy way how to find all model changes you need to fix. Please take care and make sure that you fix your code properly, especially if you are writing data into game library, since you could write bad data it if you don't adjust your scripts properly.


Method changes
---------------------

All event and other methods that previously accepted multiple arguments have been consolidated into single argument object. This is true for both plugin and script methods.

Game action controllers
---------------------

Controllers (for installing and starting games) have been reworked. See [related documentation page](../tutorials/extensions/gameActions.md) for more information about how to implement them in Playnite 9.

Metadata plugin changes
---------------------

Metadata sources no longer return data ass string but instead use `MetadataProperty` objects. See [metadata plugin page](extensions/metadataPlugins.md#MetadataProperty) for more information.

Other
---------------------

Extensions no longer log into main log file (playnite.log), but use separate file called `extensions.log`.

Script extensions
---------------------


### Upgrading PowerShell extension





#### Example


Pluigns
---------------------

Many