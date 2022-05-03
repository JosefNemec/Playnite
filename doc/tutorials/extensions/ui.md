Interacting with Playnite's UI
=====================

Getting list of selected games
---------------------

To get list of currently selected games use `MainView` from `PlayniteApi` variable. `MainView` provides [IMainViewAPI](xref:Playnite.SDK.IMainViewAPI) interface with [SelectedGames](xref:Playnite.SDK.IMainViewAPI.SelectedGames) property returning list of all selected games.

# [C#](#tab/csharp)
```csharp
var gameCount = PlayniteApi.MainView.SelectedGames.Count;
PlayniteApi.Dialogs.ShowMessage($"Selected {gameCount} games");
```

# [PowerShell](#tab/tabpowershell)
```powershell
$PlayniteApi.MainView.SelectedGames | Select -ExpandProperty Name | Out-File "SelectedGames.txt"
```
***

Custom UI elements
---------------------

See [custom UI integration](customUiIntegration.md) page for more details.

Menus
---------------------

See [menus](menus.md) page for more details.

Dialogs
---------------------

[IDialogsFactory](xref:Playnite.SDK.IDialogsFactory) API can be used to show various dialogs.

# [C#](#tab/csharp)
```csharp
PlayniteApi.Dialogs.ShowMessage("Hello world!");
```

# [PowerShell](#tab/tabpowershell)
```powershell
$PlayniteApi.Dialogs.ShowMessage("Hello world!")
```
***

Sidebar
---------------------

To provide new items to the Sidebar, override `GetSidebarItems` method and return list of [SidebarItem](xref:Playnite.SDK.Plugins.SidebarItem) items.

### Item types

There are two types of Sidebar items set via `Type` property:
- `Button`: Simple activation button.
- `View`: View button that shows custom view when clicked.

To implement `Button` type: set `Type` property of a sidebar item to `Button` and assign action to `Activated` action.

To implement `View` type: set `Type` property of a sidebar item to `View` and assign `Opened` and `Closed` actions. `Opened` is called when user clicks the item and expects UI control to be returned (which is then loaded as a new Playnite view). `Closed` is called when user switches to a different view.

### Progress indicator

Sidebar items can also show progress indicator, use `ProgressValue` and `ProgressMaximum` properties to set a specific progress state. Setting `ProgressValue` to `0` hides the progress bar.

### Example

# [C#](#tab/csharp)
```csharp
public override IEnumerable<SidebarItem> GetSidebarItems()
{
    yield return new SidebarItem
    {
        Title = "Calculator",            
        // Loads icon from plugin's installation path
        Icon = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "icon.png"),
        ProgressValue = 40,
        Activated = () => Process.Start("calc")
    };
}
```

# [PowerShell](#tab/tabpowershell)
```powershell
'Not supported in PowerShell extensions.'
```
***

Top panel
---------------------

To provide new items to the Top panel, override `GetTopPanelItems` method and return list of [TopPanelItem](xref:Playnite.SDK.Plugins.TopPanelItem) items.

# [C#](#tab/csharp)
```csharp
public override IEnumerable<TopPanelItem> GetTopPanelItems()
{
    yield return new TopPanelItem
    {
        Title = "Calculator",
        Icon = new TextBlock
        {
            Text = char.ConvertFromUtf32(0xeaf1),
            FontSize = 20,
            FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
        },
        Activated = () => Process.Start("calc")
    };
}
```

# [PowerShell](#tab/tabpowershell)
```powershell
'Not supported in PowerShell extensions.'
```
***

Supported icon formats
---------------------

Various UI objects support icon definitions and icon format is correctly not strictly enforced. Icon is an `object` type and Playnite will interpret it as:

- If `string` is provided, Playnite interprets it in the following order:
  - If application (WPF) resource with specified name is found, then it's used as icon object. WPF resources are objects you can get via [GetResource](xref:Playnite.SDK.ResourceProvider) method.
  - If absolute file path is found, Playnite will try to load it as an image.
  - If partial file path is found:
    - Theme file (file relative to currently used theme's folder) is loaded as an image if found.
    - Database file is loaded as an image if found.

- If any other type is found, Playnite assigns that object as icon's content. Passed icon object is assigned to UI item's `Content` property, so any WPF compatible UI element can be passed.

For example, if you want to use font symbol as an icon, you can assign `TextBlock`:

```csharp
new TextBlock
{
    Text = char.ConvertFromUtf32(0xeaf1),
    FontSize = 20,
    FontFamily = ResourceProvider.GetResource("FontIcoFont") as FontFamily
}
```  

`FontIcoFont` in the example references [Icofont](https://icofont.com) font pack, which is included by default in Playnite, but you can use any symbol font as long as it's installed (for example `Segoe MDL2` comes with Windows) or if you explicitly load custom font file and then reference it.