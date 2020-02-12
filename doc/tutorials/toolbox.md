# Extension Toolbox utility


Creating new extensions
---------------------

### Themes

```
Toolbox.exe new <themetype> <themename>
```

`<themetype>` available options:
- **DesktopTheme**
- **FullscreenTheme**

`<themename>` - name of the theme.

#### Example

```
Toolbox.exe new desktoptheme "New Desktop Theme"
```

### Scripts

```
Toolbox.exe new <scripttype> <scriptname> <targetfolder>
```

`<scripttype>` available options:
- **IronPythonScript**
- **PowerShellScript**

`<scriptname>` - name of the new script extension.

`<targetfolder>` - folder to create script in.

#### Example

```
Toolbox.exe new IronPythonScript "Testing Script" "d:\somefolder"
```

### Plugins

```
Toolbox.exe new <plugintype> <plugintype> <targetfolder>
```

`<plugintype>` available options:
- **GenericPlugin**
- **MetadataPlugin**
- **LibraryPlugin**

`<plugintype>` - name of the new plugin extension.

`<targetfolder>` - folder to create plugin in.

#### Example

```
Toolbox.exe new MetadataPlugin "GameDatabase metadata provider" "d:\somefolder"
```

Packing extensions
---------------------

```
Toolbox.exe pack <extensionfolder> <targetfolder>
```

`<extensionfolder>` - extension directory (theme, script or plugin) to pack (in case of plugins it has to be folder with built binaries).

`<targetfolder>` - target directory where to save packed file.

#### Example

```
Toolbox.exe pack "C:\Playnite\Themes\Fullscreen\TestingFullscreen" "c:\somefolder"
```

... will create `c:\somefolder\TestingFullscreen.pthm` package.


