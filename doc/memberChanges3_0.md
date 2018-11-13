List of class and member name changes for 3.0.0
=====================

### IGameDatabaseAPI

#### Changed

Methods accepting game identifier now accept `Guid` instead of `int`, since game Id is now being stored as `Guid`.

#### Removed

* `RemoveImage`, use `RemoveFile` instead.
* `GetFiles`, use `GetFileStoragePath` and enumerate though files manually.
* `GetFile`, use `GetFileStoragePath` or `GetFullFilePath`.
* `ImportCategories`, implement manually if needed.

#### Added

* BufferedUpdate
* GetFileStoragePath
* GetFullFilePath

### DatabaseFile

Removed completely since files are stored on disk in raw form.

### Game

#### Changed

* `Id` field changed type from `int` to `Guid`.
* `IsLaunching`, `IsInstalled`, `IsRunning`, `IsUninstalling` and `IsInstalling` are now settable.

#### Removed

* `State`, use appropriate `Is*` fields instead.

### MetadataFile

#### Changed

Constructor no longer accepts `fileId` parameter since files are stored in raw form on disk drive under automatically generated name.

### IPlugin

#### Changed

* `SettingsView` field changed to `GetSettingsView` method accepting parameter indicating whether the view is being used on first time wizard dialog.
* `Settings ` field changed to `GetSettings` method accepting parameter indicating whether the view is being used on first time wizard dialog.