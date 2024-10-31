
# <img src="https://playnite.link/applogo.png" width="32">  Playnite [![Crowdin](https://badges.crowdin.net/playnite/localized.svg)](https://crowdin.com/project/playnite)
An open source video game library manager and launcher with support for 3rd party libraries like Steam, Epic, GOG, EA App, Battle.net and [others](https://playnite.link/addons.html). Includes game emulation support, providing one unified interface for your games.

Screenshots are available at the [Homepage](http://playnite.link/)

*If you find Playnite useful please consider supporting the lead developer [Josef Nemec](https://github.com/JosefNemec) on [Patreon](https://www.patreon.com/playnite).*

Features
---------

See the [Homepage](http://playnite.link/) for the list of features.

Download
---------

Grab the latest installer or portable package from the [download](https://playnite.link/download.html) page. Playnite will automatically notify you about a new version upon release.

Requirements: Windows 7 and newer.

FAQ, Known Issues, user manual
---------
Can be found [here](https://api.playnite.link/docs/)

Questions, issues etc.
---------
If you find a bug please file an [issue](https://github.com/JosefNemec/Playnite/issues) and if relevant (crashes, broken features) please attach a diagnostics package, which can be created from inside the "About Playnite..." submenu.

Biggest community around Playnite currently gathers on our [Discord server](https://playnite.link/discord) and [Reddit](https://www.reddit.com/r/playnite/).

Privacy Statement
---------
Playnite itself doesn't store any user information and you generally don't need to provide any information to import installed games. All game library data is stored locally on your PC.

Account connection process depends on how a library plugin is implemented, but is usually done via official login web forms and only the web session cookies or tokens are stored, the same way when you login to those services via the web browser.

Add-ons
---------
Playnite can be extended with plugins (written in .NET languages), PowerShell scripts and user interface themes.

See the [extensions portal](https://api.playnite.link/docs/tutorials/index.html) for more information about how to make these addons.

Translations
---------

We use Crowdin to manage localization, please join our project if you want to submit translations:

https://crowdin.com/project/playnite

Proofreading changes to original English strings can be submitted by creating pull request for [LocSource.xaml](https://github.com/JosefNemec/Playnite/blob/devel/source/Playnite/Localization/LocSource.xaml) file.

Code Contributions
---------

**Code contributions (pull requests) are currently not being accepted while majority of code base is being rewritten for Playnite 11.**

**Please wait with any pull requests after P11 is at least in beta state.**

Please ask in the related issue first before starting implementing something to make sure that nobody else is already working on it. If an issue doesn't exist for your feature/bug fix, create one first.

Regarding code styling, there are only a few major rules:

- private fields and properties should use camelCase (without underscore)
- all methods (private and public) should use PascalCase
- use spaces instead of tabs with 4 spaces width
- add empty line between code block end `}` and additional expression
- always encapsulate the code body after *if, for, foreach, while* etc. with curly braces:

```csharp
if (true)
{
    DoSomething();
}

DoSomethingElse();
```

instead of

```csharp
if (true)
    DoSomething();
DoSomethingElse();
```

Branches
---------
* `master` - default branch representing state of currently released build.
* `devel` - development branch containing latest changes. All pull requests should be made against `devel` branch.
* `devel*` - development branches for specific features/versions.

Roadmap
---------

Playnite is currently being rewritten from scratch for next major version release 11. The work is being done in private repository until beta release, after which the code will be release in this repository under the same license as current version 10 release. There is no list of planned changes and new features for version 11.

Development
---------

See the [wiki](https://github.com/JosefNemec/Playnite/wiki/Building) for info about building and setting up the development environment.

Others
---------

.NET development tools courtesy of [JetBrains](https://www.jetbrains.com/?from=Playnite)

[![jetbrains](https://user-images.githubusercontent.com/3874087/128503701-884cdae4-3283-4d67-8ad1-6103e777a660.png)](https://www.jetbrains.com/?from=Playnite)

This program uses free code signing provided by [SignPath.io](https://signpath.io?utm_source=foundation&utm_medium=github&utm_campaign=playnite), and a free code signing certificate by the [SignPath Foundation](https://signpath.org?utm_source=foundation&utm_medium=github&utm_campaign=playnite)

[![Capture](https://user-images.githubusercontent.com/3874087/128503363-9c39f8cd-9900-4a8b-83f2-81359d4fc731.PNG)](https://about.signpath.io?utm_source=foundation&utm_medium=github&utm_campaign=playnite)
