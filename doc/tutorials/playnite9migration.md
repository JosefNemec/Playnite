Playnite 9 add-on migration guide
=====================

Extension load changes
---------------------

### Scripts

**IronPython support has been removed completely, therefore you have to rewrite those to PowerShell.**

PowerShell extensions are now imported as [PowerShell modules](https://docs.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-script-module?view=powershell-5.1). The extension of the file must be `.psm1` (or `.psd1` if you use a [PowerShell module manifest](https://docs.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-module-manifest?view=powershell-5.1)).

Any exported functions from your extension must be exported from the module. In a `.psm1` file all functions in the module scope are exported by default, but functions in the global scope (defined like `function global:OnGameStarted()`) will _not_ be correctly exported. Exported functions must accept the *exact* number of arguments that Playnite passes to them.

### Plugins

Generic plugins now have to inherit from [GenericPlugin](xref:Playnite.SDK.Plugins.GenericPlugin) class, not `Plugin` class, otherwise they won't be loaded at all.

Model changes
---------------------

Multiple changes have been made to various data models, properties removed, renamed, added. This primarily affects `Game` and `Emulator` objects.

> [!NOTE]
> Since PowerShell is not statically typed language there's no easy way how to find all model changes you need to fix. Please take care and make sure that you fix  and test your code properly, especially if you are writing data into game library, since you could write bad data it if you don't adjust your scripts properly.

File changes:

[M	source/PlayniteSDK/Models/AppSoftware.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Models_AppSoftware.cs.html)  
[M	source/PlayniteSDK/Models/CompletionStatus.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Models_CompletionStatus.cs.html)  
[M	source/PlayniteSDK/Models/DatabaseObject.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Models_DatabaseObject.cs.html)  
[M	source/PlayniteSDK/Models/Emulator.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Models_Emulator.cs.html)  
[M	source/PlayniteSDK/Models/Game.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Models_Game.cs.html)  
[M	source/PlayniteSDK/Models/GameAction.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Models_GameAction.cs.html)  
[M	source/PlayniteSDK/Models/PastTimeSegment.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Models_PastTimeSegment.cs.html)  
[M	source/PlayniteSDK/Models/Platform.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Models_Platform.cs.html)  
[M	source/PlayniteSDK/Models/Region.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Models_Region.cs.html)  
[M	source/PlayniteSDK/BuiltInExtensions.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_BuiltInExtensions.cs.html)  
[M	source/PlayniteSDK/Collections/ObservableObject.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Collections_ObservableObject.cs.html)  
[M	source/PlayniteSDK/Data/DataSerialization.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Data_DataSerialization.cs.html)  
[M	source/PlayniteSDK/Database/IGameDatabase.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Database_IGameDatabase.cs.html)  
[M	source/PlayniteSDK/Database/IItemCollection.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Database_IItemCollection.cs.html)  
[M	source/PlayniteSDK/Events/ApplicationEvents.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Events_ApplicationEvents.cs.html)  
[M	source/PlayniteSDK/Exceptions/ScriptRuntimeException.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Exceptions_ScriptRuntimeException.cs.html)  
[M	source/PlayniteSDK/ExpandableVariables.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_ExpandableVariables.cs.html)  
[M	source/PlayniteSDK/Extensions/ListExtensions.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Extensions_ListExtensions.cs.html)  
[M	source/PlayniteSDK/IDialogsFactory.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_IDialogsFactory.cs.html)  
[M	source/PlayniteSDK/ILogger.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_ILogger.cs.html)  
[M	source/PlayniteSDK/IMainViewAPI.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_IMainViewAPI.cs.html)  
[M	source/PlayniteSDK/IPlayniteAPI.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_IPlayniteAPI.cs.html)  
[M	source/PlayniteSDK/IPlayniteSettingsAPI.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_IPlayniteSettingsAPI.cs.html)  
[M	source/PlayniteSDK/IWebView.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_IWebView.cs.html)  
[M	source/PlayniteSDK/LogManager.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_LogManager.cs.html)  
[M	source/PlayniteSDK/MetadataProvider.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_MetadataProvider.cs.html)  
[M	source/PlayniteSDK/Plugins/LibraryPlugin.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Plugins_LibraryPlugin.cs.html)  
[M	source/PlayniteSDK/Plugins/MetadataPlugin.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Plugins_MetadataPlugin.cs.html)  
[M	source/PlayniteSDK/Plugins/Plugin.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Plugins_Plugin.cs.html)  
[M	source/PlayniteSDK/Properties/AssemblyInfo.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_Properties_AssemblyInfo.cs.html)  
[M	source/PlayniteSDK/RelayCommand.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_RelayCommand.cs.html)  
[M	source/PlayniteSDK/ResourceProvider.cs](https://playnite.link/sdkchangelog/5.6.0-6.0.0/source_PlayniteSDK_ResourceProvider.cs.html)  

Method changes
---------------------

All event and other methods that previously accepted multiple arguments have been consolidated into single argument object. This is true for both plugin and script methods.

Game action controllers
---------------------

Controllers (for installing/uninstalling and starting games) have been reworked. See [related documentation page](../tutorials/extensions/gameActions.md) for more information about how to implement them in Playnite 9.

Metadata plugin changes
---------------------

Metadata sources no longer return data as strings but instead use `MetadataProperty` objects. See [metadata plugin page](extensions/metadataPlugins.md#metadataproperty) for more information.

Other
---------------------

Extensions no longer log into main log file (playnite.log), but use separate file called `extensions.log`.

Themes
---------------------

Use Toolbox utility to update theme files to new version, this is absolutely necessary otherwise Blend will no longer load your theme properly. Note that some styles/views are no longer available, others have been moved or renamed. There are also new styles added to support styling of controls that were previously not exposed to themes (like filter panel dropdowns and search boxes).

Playnite 9 also changes how theme files are loaded, which should solve inheritance issue with static references. For example if you had custom CheckBox style then changes from global CheckBox style would not be applied to inherited style and you had to define the whole inherited style. This is no longer needed and theme global styles should be inherited properly.

File changes:

To see what letters before file change mean check [this page](https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---diff-filterACDMRTUXB82308203).

[M	source/Playnite.DesktopApp/Themes/Desktop/Default/Constants.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_Constants.xaml.html)  
A	source/Playnite.DesktopApp/Themes/Desktop/Default/CustomControls/ComboBoxList.xaml  
A	source/Playnite.DesktopApp/Themes/Desktop/Default/CustomControls/ExtendedDataGrid.xaml  
A	source/Playnite.DesktopApp/Themes/Desktop/Default/CustomControls/FilterSelectionBox.xaml  
D	source/Playnite.DesktopApp/Themes/Desktop/Default/CustomControls/MainMenu.xaml  
D	source/Playnite.DesktopApp/Themes/Desktop/Default/CustomControls/NullableIntBox.xaml  
D	source/Playnite.DesktopApp/Themes/Desktop/Default/CustomControls/NumericBox.xaml  
A	source/Playnite.DesktopApp/Themes/Desktop/Default/CustomControls/NumericBoxes.xaml  
A	source/Playnite.DesktopApp/Themes/Desktop/Default/CustomControls/SearchBox.xaml  
[M	source/Playnite.DesktopApp/Themes/Desktop/Default/CustomControls/SidebarItem.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_CustomControls_SidebarItem.xaml.html)  
A	source/Playnite.DesktopApp/Themes/Desktop/Default/CustomControls/TopPanelItem.xaml  
D	source/Playnite.DesktopApp/Themes/Desktop/Default/CustomControls/ViewSettingsMenu.xaml  
A	source/Playnite.DesktopApp/Themes/Desktop/Default/DefaultControls/DataGrid.xaml  
[M	source/Playnite.DesktopApp/Themes/Desktop/Default/DefaultControls/Expander.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_DefaultControls_Expander.xaml.html)  
[M	source/Playnite.DesktopApp/Themes/Desktop/Default/DefaultControls/TabControl.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_DefaultControls_TabControl.xaml.html)  
A	source/Playnite.DesktopApp/Themes/Desktop/Default/DerivedStyles/GridViewGroupStyle.xaml  
[M	source/Playnite.DesktopApp/Themes/Desktop/Default/DerivedStyles/PlayButton.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_DerivedStyles_PlayButton.xaml.html)  
[M	source/Playnite.DesktopApp/Themes/Desktop/Default/DerivedStyles/PropertyItemButton.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_DerivedStyles_PropertyItemButton.xaml.html)  
[M	source/Playnite.DesktopApp/Themes/Desktop/Default/Media.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_Media.xaml.html)  
[M	source/Playnite.DesktopApp/Themes/Desktop/Default/Views/DetailsViewGameOverview.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_Views_DetailsViewGameOverview.xaml.html)  
D	source/Playnite.DesktopApp/Themes/Desktop/Default/Views/FilterPanel.xaml  
A	source/Playnite.DesktopApp/Themes/Desktop/Default/Views/FilterPanelView.xaml  
[M	source/Playnite.DesktopApp/Themes/Desktop/Default/Views/GridViewGameOverview.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_Views_GridViewGameOverview.xaml.html)  
[M	source/Playnite.DesktopApp/Themes/Desktop/Default/Views/Library.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_Views_Library.xaml.html)  
[M	source/Playnite.DesktopApp/Themes/Desktop/Default/Views/LibraryDetailsView.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_Views_LibraryDetailsView.xaml.html)  
[M	source/Playnite.DesktopApp/Themes/Desktop/Default/Views/LibraryGridView.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_Views_LibraryGridView.xaml.html)  
D	source/Playnite.DesktopApp/Themes/Desktop/Default/Views/MainPanel.xaml  
[M	source/Playnite.DesktopApp/Themes/Desktop/Default/Views/Sidebar.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.DesktopApp_Themes_Desktop_Default_Views_Sidebar.xaml.html)  
A	source/Playnite.DesktopApp/Themes/Desktop/Default/Views/TopPanel.xaml  
D	source/Playnite.DesktopApp/Themes/Desktop/DefaultRed/Constants.xaml  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Constants.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_Constants.xaml.html)  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/CustomControls/FilterDbItemtSelection.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_CustomControls_FilterDbItemtSelection.xaml.html)  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/CustomControls/FilterEnumListSelection.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_CustomControls_FilterEnumListSelection.xaml.html)  
A	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/CustomControls/FilterPresetSelector.xaml  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/CustomControls/FilterStringListSelection.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_CustomControls_FilterStringListSelection.xaml.html)  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/CustomControls/WindowBase.xaml  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DefaultControls/ScrollViewer.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_DefaultControls_ScrollViewer.xaml.html)  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DefaultControls/Slider.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_DefaultControls_Slider.xaml.html)  
A	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DefaultControls/ToggleButton.xaml  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DefaultControls/ToolTip.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_DefaultControls_ToolTip.xaml.html)  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/ButtonBottomMenu.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_DerivedStyles_ButtonBottomMenu.xaml.html)  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/ButtonFilterNagivation.xaml  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/ButtonMainMenu.xaml  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/ButtonMessageBox.xaml  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/ButtonTopMenu.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_DerivedStyles_ButtonTopMenu.xaml.html)  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/ButtonVirtualKeyboard.xaml  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/CheckBoxSettings.xaml  
A	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/ItemFilterQuickPreset.xaml  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/ListGameItem.xaml  
A	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/ListGameItemStyle.xaml  
A	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/ListGameItemTemplate.xaml  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/MainWindowStyle.xaml  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/DerivedStyles/ToggleButtonTopFilter.xaml  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Images/ButtonPrompts/PlayStation/PlayStation.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_Images_ButtonPrompts_PlayStation_PlayStation.xaml.html)  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Images/ButtonPrompts/Xbox/Xbox.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_Images_ButtonPrompts_Xbox_Xbox.xaml.html)  
A	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/ActionSelection.xaml  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/Filters.xaml  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/FiltersAdditional.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_Views_FiltersAdditional.xaml.html)  
A	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/FiltersView.xaml  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/GameDetails.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_Views_GameDetails.xaml.html)  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/GameMenu.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_Views_GameMenu.xaml.html)  
A	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/GameStatus.xaml  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/Main.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_Views_Main.xaml.html)  
[M	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/MainMenu.xaml](https://playnite.link/themechangelog/1.9.0-2.0.0/source_Playnite.FullscreenApp_Themes_Fullscreen_Default_Views_MainMenu.xaml.html)  
A	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/MessageBox.xaml  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/Notifications.xaml  
A	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/NotificationsMenu.xaml  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/SettingsMenu.xaml  
A	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/SettingsMenus.xaml  
A	source/Playnite.FullscreenApp/Themes/Fullscreen/Default/Views/TextInput.xaml  
D	source/Playnite.FullscreenApp/Themes/Fullscreen/DefaultLime/Constants.xaml  

