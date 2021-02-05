UI Integration
=====================

Introduction
---------------------


Implementation
---------------------

### User Control implementation

1) Add new standard WPF UserControl into your project.
2) Change base class from `UserControl` to `PluginUserControl`

You should end up with something like this:

```xml
<PluginUserControl x:Class="TestPlugin.TestPluginUserControl"
                   xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                   xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
    </Grid>
</PluginUserControl>
```

```csharp
public partial class TestPluginUserControl : PluginUserControl
{
    public TestPluginUserControl()
    {
        InitializeComponent();
    }
}
```

### Registering custom control

For new control to be recognized by Playnite API and themes call [AddCustomElementSupport](xref:Playnite.SDK.Plugins.Plugin.AddCustomElementSupport(Playnite.SDK.Plugins.AddCustomElementSupportArgs)) method in plugin's constructor.

AddCustomElementSupportArgs contains several arguments:

|Property|Description|
|:---|:---|
|ElementList| List of element names supported by the plugin. |
|SourceName| Plugin source name for element name references and settings bindings. |
|SettingsRoot| Binding root path if you want to allow themes to reference [plugin settings](#plugin-settings). |

Following example will allow themes to reference plugin controls by `TestPlugin_TestUserControl1` and `TestPlugin_TestUserControl2` names:

```csharp
AddCustomElementSupport(new AddCustomElementSupportArgs
{
    ElementList = new List<string> { "TestUserControl1", "TestUserControl2" },
    SourceName = "TestPlugin"
});
```

Lastly, override [GetGameViewControl](xref:Playnite.SDK.Plugins.Plugin.GetGameViewControl(Playnite.SDK.Plugins.GetGameViewControlArgs)) method. The method is called when theme view template is initialized and plugin control should be "injected" into the view.

Example:
```csharp
public override Control GetGameViewControl(GetGameViewControlArgs args)
{
    if (args.Name == "TestUserControl")
    {
        return new TestPluginUserControl();
    }

    return null;
}
```

### Handling data context

Data of currently bound game object can be accessed via PluginUserControl's `GameContext` property. If you want to react to data context changes, you can override [GameContextChanged](xref:Playnite.SDK.Controls.PluginUserControl.GameContextChanged(Playnite.SDK.Models.Game,Playnite.SDK.Models.Game)) method.

Theme integration
---------------------

To actually use plugin control in a view, add `ContentControl` with its name set in `<SourceName>_<ElementName>` format.

For example, to include `TestPlugin_TestUserControl1` from above example, add this to a view:

```xml
<ContentControl x:Name="TestPlugin_TestUserControl1" />
```

Plugin controls can be currently used in following views:

|Desktop mode | Fullscreen mode |
|:---|:---|
|DetailsViewGameOverview|GameDetails|
|GridViewGameOverview|ListGameItemTemplate|
|GridViewItemTemplate||
|DetailsViewItemTemplate||

### Checking if plugin is installed

You can use `PluginStatus` markup to add conditions based on if a plugin is installed or not.

```xml
{PluginStatus Plugin=AddonId, Status=Installed}
```

Plugin settings
---------------------

If a plugin specifies `SettingsRoot` property when calling `AddCustomElementSupport`, themes can reference plugin's settings via `PluginSettings` markup.

```xml
<TextBlock Text="{PluginSettings Plugin=<SourceName>, Path=CustomOption}" />
```

`SettingsRoot` must be relative binding path to the plugin class. For example, if your plugin class stores settings in "Settings" property, then SettingsRoot should be set to just "Settings". If you want themes to react to settings changes, then you need to implement `INotifyPropertyChanged`.