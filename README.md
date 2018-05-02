
# <img src="https://github.com/JosefNemec/Playnite/raw/master/web/applogo.png" width="32">  Playnite
Open source video game library manager and launcher with support for 3rd party libraries like Steam, GOG, Origin, Battle.net and Uplay. Including game emulation support, providing one unified interface for your games.

Screenshots on [Homepage](http://playnite.link/)

*If you find Playnite useful please consider supporting the lead developer [Josef Nemec](https://github.com/JosefNemec) on [Patreon](https://www.patreon.com/playnite).*

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

Privacy Statement
---------
Playnite doesn't store any user information and you don't need to provide any information to import installed games. Login is required only for full library import of GOG, Origin and Battle.net games and in that case only web session cookie is stored, the same way when you login to those services via web browser.

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

You can see planned version with their features in [projects overview](https://github.com/JosefNemec/Playnite/projects).

Development
---------

See [wiki](https://github.com/JosefNemec/Playnite/wiki/Building) about building and settings up development environment.