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