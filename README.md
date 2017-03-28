
# <img src="https://github.com/JosefNemec/Playnite/raw/master/web/applogo.png" width="32">  Playnite
Video game library manager and launcher with support for 3rd party libraries like Steam, GOG and Origin, providing one unified interface for your games.

Screenshots on [Homepage](http://playnite.link/)

### Features in current Alpha release:

**Steam, Origin and GOG support**

Import games from Steam, Origin and GOG services including games that are not installed.

**View options**

Choose from 3 different view options.

**Custom games**

Add any game or program with custom launch options.

**Automatic Update**

Application automatically updates to new version.

**Portable installation**

Run Playnite without need to install, with ability to run from any place, with option to configure database location for automatic sync via service like DropBox or Google Drive.

Download
---------

Grab latest installer or portable package from [releases](https://github.com/JosefNemec/Playnite/releases) page. Playnite will automatically notify about new version when released.

Requirements: Windows 7, 8 or 10 and [.Net 4.6](https://www.microsoft.com/en-us/download/details.aspx?id=53344)

Questions, issues etc.
---------
If you find a bug please file an [issue](https://github.com/JosefNemec/Playnite/issues) and if relevant (crashes, broken features) please attach diagnostics package, which can be created via from "About Playnite..." menu.

If you have any question related mainly to development then check out [Playnite Gitter room](https://gitter.im/Playnite/Lobby?utm_source=share-link&utm_medium=link&utm_campaign=share-link).

Building
---------

Solution will properly load only in Visual Studio 2017 because it contains ASP.NET Core project with .csproj project configuration, which is not supported in 2015. Otherwise there are no other requirements to build from VS, all references should be downloaded from NuGet.

### Build scripts
To build from cmdline run **build.ps1** in PowerShell, script builds Release configuration by default into the same directory. Script accepts *Configuration*, *OutputPath*, *Setup*, *Portable* and *SkipBuild* parameters.

### Building installer
[NSIS 3+](http://nsis.sourceforge.net/Main_Page) with [NsProcess plugin](http://nsis.sourceforge.net/NsProcess_plugin) is required to build installer. To build installer run build script with **-Setup** parameter:
``` .\build.ps1 -Setup ```

Portable zip package can be built when using **-Portable** parameter.

Development
---------

Playnite runs in development environment without extra configuration with exception of Steam library import and auto-update features.

### Auto-Update
Requires simple web server serving **update.json** file with information about available version. URL with update.json has to be configured in **app.(Debug|Release).config** file, **UpdateUrl** key.

Example file:
```
{
    "stable" : {
        "version" : "0.70.0.0",
        "url" : "https://localhost/build/PlayniteInstaller.exe"
    }
}
```

### Steam import
In order to download information about full Steam library (installed games can be imported without this), Steam API has to be used with proper API key obtained from Valve. Since API access comes with some limitations (1 request per second and 100k requests per day), Playnite doesn't directly access Steam API, but uses caching service. This also prevents distribution of API keys to end users.

To deploy caching service in development environment, you must do following:
* Build **PlayniteServices** project, either manually from VS, using **buildServices.ps1** script or via ```dotnet publish```
* Create configuration file with Steam API key called **apikeys.json** inside service folder
* Deploy project to [web server](https://docs.microsoft.com/en-us/aspnet/core/publishing/) or run it directly via ```dotnet .\PlayniteServices.dll```
* Configure root endpoint inside **app.(Debug|Release).config** file, **ServicesUrl** key.

apikeys.json example:
```
{
    "Steam" : "25SSE201224DEE54F732DDDC2BA21690C"
}
```

Contributions
---------

Any contributions are welcome!

Checkout [Roadmap](https://github.com/JosefNemec/Playnite/wiki/Roadmap) on wiki to see features that are currently planned or in development.

Regarding code styling, there are only a few major rules:
* private fields and properties should use camelCase (without underscore)
* all methods (private and public) should use PascalCase
* use spaces instead of tabs with 4 spaces width
* always encapsulate with brackets:
```
if (true)
{
    DoSomething()
}
```
instead of 
```
if (true)
    DoSomething()
```
