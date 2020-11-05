Plugin settings
=====================

Basics
---------------------

Plugins can provide configuration view that end users can use to change plugin's behavior. This includes UI component to display settings objects and input verification methods.

Implementation
---------------------

Settings support requires that you override methods `GetSettings` and `GetSettingsView` methods. `GetSettings` returns `ISettings` object containing the settings data, while `GetSettingsView` returns WPF `UserControl` used to display that data.

### 1. Settings class

Following example show settings object implementing two custom plugin setting options and sets their default value.

```csharp
public class TestPluginSettings : ISettings
{
    public string Option1 { get; set; } = string.Empty;

    public bool Option2 { get; set; } = false;
}
```

### 2. Implementing ISettings

Our example class is not complete since we don't have an implementation for all `ISettings` methods. Following example show all methods required for complete `ISettings` implementation.

```csharp
public void BeginEdit()
{
    // Code executed when settings view is opened and user starts editing values.
}

public void CancelEdit()
{
    // Code executed when user decides to cancel any changes made since BeginEdit was called.
    // This method should revert any changes made to Option1 and Option2.
}

public void EndEdit()
{
    // Code executed when user decides to confirm changes made since BeginEdit was called.
    // This method should save settings made to Option1 and Option2.
}

public bool VerifySettings(out List<string> errors)
{
    // Code execute when user decides to confirm changes made since BeginEdit was called.
    // Executed before EndEdit is called and EndEdit is not called if false is returned.
    // List of errors is presented to user if verification fails.
}
```

### 3. Implementing settings view

Settings view is must be standard WPF UserControl. To add new control into your plugin project:

- Right click on your project, select **Add** -> **New Item...**.
- Select **WPF** from the list of control templates and choose **User Control (WPF)** template.
- Change name of the control to something like `NameOfMyPluginSettingsView.xaml` and save.
- Open `NameOfMyPluginSettingsView.xaml` and implement the view.

Plugin's `ISettings` object is set by Playnite as data context while the view is being created (when user starts editing the settings), so you can directly bind it.

Following example show implementation of text field for our `Option1` value and check box for `Option2`.

```xml
<UserControl x:Class="TestPlugin.TestPluginSettingsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">  
    <StackPanel>
        <TextBlock Text="Description for Option1:" />
        <TextBox Text="{Binding Option1}" />
        <TextBlock Text="Description for Option2:" />
        <CheckBox IsChecked="{Binding Option2}" />
    </StackPanel>
</UserControl>
```

### 4. Hooking everything to a plugin class

As stated before, plugin class must override `GetSettings` and `GetSettingsView` methods.

```csharp
public class TestPlugin : Plugin // or LibraryPlugin for library plugins, implementation is the same.
{    
    public override ISettings GetSettings(bool firstRunSettings)
    {
        return new TestPluginSettings();
    }
    
    public override UserControl GetSettingsView(bool firstRunSettings)
    {
        return new TestPluginSettingsView();
    }
}
```

Saving and loading settings
---------------------

To store your settings permanently you have to implement some logic that will store data into some permanent storage and load them then time your plugin is loaded. You can do this manually or use built-in method that Playnite provides for this purpose.

Following example shows how to load and save values for your plugin.

```csharp
public class TestPluginSettings : ISettings
{
    private TestPlugin plugin;

    public string Option1 { get; set; } = string.Empty;

    public bool Option2 { get; set; } = false;

    // Playnite serializes settings object to a JSON object and saves it as text file.
    // If you want to exclude some property from being saved then use `JsonIgnore` ignore attribute.
    [JsonIgnore]
    public bool OptionThatWontBeSaved { get; set; } = false;

    // Parameterless constructor must exist if you want to use LoadPluginSettings method.
    public TestPluginSettings()
    {
    }

    public TestPluginSettings(TestPlugin plugin)
    {
        // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
        this.plugin = plugin;

        // Load saved settings.
        var savedSettings = plugin.LoadPluginSettings<TestPluginSettings>();

        // LoadPluginSettings returns null if not saved data is available.
        if (savedSettings != null)
        {
            Option1 = savedSettings.Option1;
            Option2 = savedSettings.Option2;
        }
    }

    // To save settings just call SavePluginSettings when user confirms changes.
    public void EndEdit()
    {
        plugin.SavePluginSettings(this);
    }
}

```

Settings workflow
---------------------

When user opens settings window in Playnite, following happens with your plugin:

- Playnite gets `GetSettings` and `GetSettingsView` values from your plugin.
- `Settings` object is set as `DataContext` of `SettingsView` view.
- `BeginEdit` method is called.

- When user decides to cancel editing of settings:
    - `CancelEdit` is called.

- When use decides to confirm changes:
    - `VerifySettings` is called:
        - If `false` is returned Playnite shows verification errors to a user.
        - If `true` is returned `EndEdit` is called.