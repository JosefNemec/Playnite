
# <img src="https://github.com/JosefNemec/Playnite/raw/master/web/applogo.png" width="32">  Playnite
Open source video game library manager and launcher with support for 3rd party libraries like Steam, GOG, Origin, Battle.net and Uplay. Including game emulation support, providing one unified interface for your games.

Screenshots on [Homepage](http://playnite.link/)

Features
---------

See [homepage](http://playnite.link/) for list of features.

Download
---------

Grab latest installer or portable package from [releases](https://github.com/JosefNemec/Playnite/releases) page. Playnite will automatically notify about new version when released.

Requirements: Windows 7, 8 or 10 and [.Net 4.6](https://www.microsoft.com/en-us/download/details.aspx?id=53344)

Portable version requires [Microsoft Visual C++ 2013 Redistributable (x86)](https://www.microsoft.com/en-us/download/details.aspx?id=40784) to be installed manually.

Extensions
---------
Playnite can be extended with plugins (written in .NET languages) or by scripts (PowerShell and IronPython are currently supported).

See [extensions portal](https://playnite.link/docs/) for tutorials and full API documentation.

Known Issues
---------
List of known issues and solutions can be found [on wiki](https://github.com/JosefNemec/Playnite/wiki/Known-Issues).

Security Concerns
---------
Playnite doesn't store any user information and you don't need to provide any information to import installed games. Login is required only for full library import of GOG and Origin games and in that case only web session cookie is stored, the same way when you login to those services via web browser.

Questions, issues etc.
---------
If you find a bug please file an [issue](https://github.com/JosefNemec/Playnite/issues) and if relevant (crashes, broken features) please attach diagnostics package, which can be created via from "About Playnite..." menu.

General discussion lives on [Reddit](https://www.reddit.com/r/playnite/) and you can also ask a question directly on [Discord](https://discord.gg/hSFvmN6) or follow [@AppPlaynite](https://twitter.com/AppPlaynite) for updates.

Contributions
---------

### Translations
See [How to: Translations](https://github.com/JosefNemec/Playnite/wiki/How-to:-Translations) wiki page.

### Themes
See [How to: Themes](https://github.com/JosefNemec/Playnite/wiki/How-to%3A-Themes) wiki page.

### Code Contributions
Please ask in an issue first before starting with implementation to make sure that nobody else is already working on it. If the issue doesn't exists for your feature/bug fix, create one first.

Regarding code styling, there are only a few major rules:

- private fields and properties should use camelCase (without underscore)
- all methods (private and public) should use PascalCase
- use spaces instead of tabs with 4 spaces width
- always encapsulate with brackets:

````
if (true)
{
    DoSomething()
}
````
instead of
```
if (true)
    DoSomething()
```

Roadmap
---------

You can see planned version with their feature in [projects overview](https://github.com/JosefNemec/Playnite/projects).


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

Playnite runs in development environment without extra configuration with exception of Steam library import, IGDB integration and auto-update features.

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
    "SteamKey" : "25SSE201224DEE54F732DDDC2BA21690C"
}
```
### IGDB integration
Similarly to Steam integration, IGDB services require API key to be configured in `apikeys.json` file under `IGBDKey` property.
