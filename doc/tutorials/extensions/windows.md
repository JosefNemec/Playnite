Creating custom windows
=====================

Intro
---------------------

Manually created windows will not inherit Playnite theme, you need to use [CreateWindow](xref:Playnite.SDK.IDialogsFactory.CreateWindow(Playnite.SDK.WindowCreationOptions)) to create new window instance. `CreateWindow` returns new instance of WPF Window class.

Examples
---------------------

# [C#](#tab/csharp)
```csharp
var window = PlayniteApi.Dialogs.CreateWindow(new WindowCreationOptions
{
    ShowMinimizeButton = false
});

window.Height = 1024;
window.Width = 768;
window.Title = "Some title";

// Set content of a window. Can be loaded from xaml, loaded from UserControl or created from code behind
window.Content = 

// Set data context if you want to use MVVM pattern
window.DataContext = 

// Set owner if you need to create modal dialog window
window.Owner = PlayniteApi.Dialogs.GetCurrentAppWindow();
window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

// Use Show or ShowDialog to show the window
window.ShowDialog();
```

# [PowerShell](#tab/tabpowershell)
```powershell
$windowCreationOptions = New-Object Playnite.SDK.WindowCreationOptions
$windowCreationOptions.ShowMinimizeButton = $false

$window = $PlayniteApi.Dialogs.CreateWindow($windowCreationOptions);
$window.Height = 768;
$window.Width = 768;
$window.Title = "Some title";

# Set content of a window. Can be loaded from xaml, loaded from UserControl or created from code behind
[xml]$xaml = @"
<UserControl
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <UserControl.Resources>
        <Style TargetType="TextBlock" BasedOn="{StaticResource BaseTextBlockStyle}" />
    </UserControl.Resources>

    <StackPanel Margin="15,0,0,0">
        <StackPanel Orientation="Horizontal">
            <TextBlock Text="An example button:" Margin="0,0,15,0" VerticalAlignment="Center"/>
            <Button x:Name="MyButton" Content="Click me!"/>
        </StackPanel>
        <TextBlock Text="Currently selected games:" Margin="0,15,0,0"/>
        <ItemsControl ItemsSource="{Binding}">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding Path=Name}" Margin="15,0,0,0"
                        Style="{StaticResource BaseTextBlockStyle}"/>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
        <StackPanel Orientation="Horizontal" Margin="0,15,0,0">
            <TextBlock Text="Total selected games: "/>
            <TextBlock Text="{Binding .Count}"/>
        </StackPanel>
    </StackPanel>
</UserControl>
"@
$reader = [System.Xml.XmlNodeReader]::new($xaml)
$window.Content = [Windows.Markup.XamlReader]::Load($reader)

# Set data context if you want to use MVVM pattern
$window.DataContext = $PlayniteApi.MainView.SelectedGames

# Attach a click event handler
$button = $window.Content.FindName("MyButton")
$button.Add_Click({
    $button.Content = "Clicked"
})

# Set owner if you need to create modal dialog window
$window.Owner = $PlayniteApi.Dialogs.GetCurrentAppWindow();
$window.WindowStartupLocation = "CenterOwner";

# Use Show or ShowDialog to show the window
$window.ShowDialog();
```