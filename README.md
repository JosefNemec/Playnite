
# <img src="https://github.com/JosefNemec/Playnite/raw/master/web/applogo.png" width="32">  Playnite
An open source video game library manager and launcher with support for 3rd party libraries like Steam, GOG, Origin, Battle.net and Uplay. Includes game emulation support, providing one unified interface for your games.

Screenshots are available at the [Homepage](http://playnite.link/)

*If you find Playnite useful please consider supporting the lead developer [Josef Nemec](https://github.com/JosefNemec) on [Patreon](https://www.patreon.com/playnite).*

Features
---------

See the [Homepage](http://playnite.link/) for the list of features.

Download
---------

Grab the latest installer or portable package from the [releases](https://github.com/JosefNemec/Playnite/releases) page. Playnite will automatically notify you about a new version upon release.

Requirements: Windows 7, 8 or 10 and [.NET Framework 4.6](https://www.microsoft.com/en-us/download/details.aspx?id=53344)

Extensions
---------
Playnite can be extended with plugins (written in .NET languages) or by scripts (PowerShell and IronPython are currently supported).

See the [extensions portal](https://playnite.link/docs/) for tutorials and the full API documentation.

FAQ
---------
Can be found [on the wiki](https://github.com/JosefNemec/Playnite/wiki/Frequently-Asked-Questions)

Known Issues
---------
The list of known issues and solutions can be found [on the wiki](https://github.com/JosefNemec/Playnite/wiki/Known-Issues).

Privacy Statement
---------
Playnite doesn't store any user information and you don't need to provide any information to import installed games. Login is required only for the full library import of GOG, Origin and Battle.net games and in that case only the web session cookie is stored, the same way when you login to those services via the web browser.

Questions, issues etc.
---------
If you find a bug please file an [issue](https://github.com/JosefNemec/Playnite/issues) and if relevant (crashes, broken features) please attach a diagnostics package, which can be created from inside the "About Playnite..." submenu.

General discussion lives on [Reddit](https://www.reddit.com/r/playnite/) and you can also ask a question directly on [Discord](https://discord.gg/hSFvmN6) or follow [@AppPlaynite](https://twitter.com/AppPlaynite) for updates.

Contributions
---------
### Translations
See the [How to: Translations](https://github.com/JosefNemec/Playnite/wiki/How-to:-Translations) wiki page.

### Themes
See the [How to: Themes](https://github.com/JosefNemec/Playnite/wiki/How-to%3A-Themes) wiki page.

### Code Contributions
Please ask in the related issue first before starting implementing something to make sure that nobody else is already working on it. If an issue doesn't exist for your feature/bug fix, create one first.

Regarding code styling, there are only a few major rules:

- private fields and properties should use camelCase (without underscore)
- all methods (private and public) should use PascalCase
- use spaces instead of tabs with 4 spaces width
- always encapsulate the code body after *if, for, foreach, while* etc. with curly braces:

```csharp
if (true)
{
    DoSomething()
}
```

instead of

```csharp
if (true)
    DoSomething()
```

Roadmap
---------

You can see the planned versions with their features in the [projects overview](https://github.com/JosefNemec/Playnite/projects).

Development
---------

See the [wiki](https://github.com/JosefNemec/Playnite/wiki/Building) for info about building and setting up the development environment.
