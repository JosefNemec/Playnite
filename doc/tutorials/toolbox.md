# Extension Toolbox utility

Introduction
---------------------

Toolbox is a Playnite utility that can be used for various tasks, mainly for creating extensions and themes. Toolbox is distributed with every Playnite installation and can be found in Playnite's installation directory.

Creating new extensions
---------------------

### Themes

```cmd
Toolbox.exe new <themetype> <themename>
```

`<themetype>` available options:
- **DesktopTheme**
- **FullscreenTheme**

`<themename>` - name of the theme.

#### Example

```cmd
Toolbox.exe new desktoptheme "New Desktop Theme"
```

### Scripts

```cmd
Toolbox.exe new <scripttype> <scriptname> <targetfolder>
```

`<scripttype>` available options:
- **PowerShellScript**

`<scriptname>` - name of the new script extension.

`<targetfolder>` - folder to create script in.

#### Example

```cmd
Toolbox.exe new PowerShellScript "Testing Script" "d:\somefolder"
```

### Plugins

```cmd
Toolbox.exe new <plugintype> <pluginname> <targetfolder>
```

`<plugintype>` available options:
- **GenericPlugin**
- **MetadataPlugin**
- **LibraryPlugin**

`<pluginname>` - name of the new plugin extension.

`<targetfolder>` - folder to create plugin in.

#### Example

```cmd
Toolbox.exe new MetadataPlugin "GameDatabase metadata provider" "d:\somefolder"
```

Packing extensions
---------------------

```cmd
Toolbox.exe pack <extensionfolder> <targetfolder>
```

`<extensionfolder>` - extension directory (theme, script or plugin) to pack (in case of plugins it has to be folder with built binaries).

`<targetfolder>` - target directory where to save packed file.

#### Example

```cmd
Toolbox.exe pack "C:\Playnite\Themes\Fullscreen\TestingFullscreen" "c:\somefolder"
```

... will create `c:\somefolder\TestingFullscreen.pthm` package.

Verify manifests
---------------------

```cmd
Toolbox.exe verify <manifest_type> <manifest_past>
```

`<manifest_type>` - `addon` for addon browser manifest or `installer` for package installer manifest. `addon` verifies linked installer manifest automatically.

`<manifest_past>` - Path to manifest yaml file. Local full path or HTTP URLs are supported.

```cmd
Toolbox.exe verify <manifest_type> <manifest_past>
```