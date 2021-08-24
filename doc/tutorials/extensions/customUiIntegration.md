Custom UI Integration
=====================

Introduction
---------------------

Extensions can expose custom UI elements for themes to integrate, this requires support by both an extension and a theme. For theme implementation part, see [this page](../themes/extensionIntegration.md).

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

`AddCustomElementSupportArgs` contains several arguments:

|Property|Description|
|:---|:---|
|ElementList| List of element names supported by the plugin. |
|SourceName| Plugin source name for element name references and settings bindings. |

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

Exposing extension settings
---------------------

You can provide easy way for themes to reference extension settings by calling [AddSettingsSupport](xref:Playnite.SDK.Plugins.Plugin.AddSettingsSupport(Playnite.SDK.Plugins.AddSettingsSupportArgs)) method in plugin's constructor, similarly to how custom element support is done.

`AddSettingsSupport` contains several arguments:

|Property|Description|
|:---|:---|
|SourceName| Plugin source name for element name references and settings bindings. |
|SettingsRoot| Binding root path relative to a plugin object instance. |

`SettingsRoot` must be relative binding path to the plugin class. For example, if your plugin class stores settings in `Settings` property, then SettingsRoot should be set to just "Settings". If you want themes to dynamically react to settings changes, then you need to implement `INotifyPropertyChanged` on your settings object.

Example:
```csharp
AddSettingsSupport(new AddSettingsSupportArgs
{
    SourceName = "TestPlugin",
    SettingsRoot = $"binding.path.relative.to.plugin.object"
});
```

Themes can then reference settings via `PluginSettings` markup. See [theme integration page](../themes/extensionIntegration.md#extension-settings) for more details.

Theme integration
---------------------

See [theme integration page](../themes/extensionIntegration.md) for more details.

As an extension developer you should provide following information to theme developer:

`Source name`: For both general UI integration and settings integration.
`Element list`: List of element names that you extension exposes and themes can integrate.
`Settings list`: If your extension exposes settings that can be used by themes.
`Addon id`: Addon id used in extension manifest, for theme developers to be able to [detect](../themes/extensionIntegration.md#detecting-if-an-extension-is-installed) if your extension is installed or not.