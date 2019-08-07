Working with Game Database
=====================

Introduction
---------------------
Game database API allows you to access and modify game library and its objects (including platforms and emulators). Database API can be accessed via `PlayniteAPI.Database` property, which provides [IDatabaseAPI](xref:Playnite.SDK.IGameDatabaseAPI) interface.

Handling Games
---------------------

### Getting Games

To get list of all games in library use [Database](xref:Playnite.SDK.IPlayniteAPI.Database) property from `IPlayniteAPI` and [Games](xref:Playnite.SDK.IGameDatabase.Games) collection.

**PowerShell**:

```powershell
# Get all games
$games = $PlayniteApi.Database.Games
# Get game with known Id
$game = $PlayniteApi.Database.Games[SomeGuidId]
```

**IronPython**:

```python
# Get all games
games = PlayniteApi.Database.Games
# Get game with known Id
game = PlayniteApi.Database.Games[SomeGuidId]
```

### Adding New Game

To add a new game create new instance of [GameInfo](xref:Playnite.SDK.Models.GameInfo) class and call `Add` method from [Games](xref:Playnite.SDK.IGameDatabase.Games) collection.

**PowerShell**:

```powershell
$newGame = New-Object "Playnite.SDK.Models.GameInfo"
$newGame.Name = "New Game"
$PlayniteApi.Database.Games.Add($newGame)
```

**IronPython**:

```python
new_game = GameInfo()
new_game.Name = "New Game"
PlayniteApi.Database.Games.Add(new_game)
```

### Changing Game Data

Changing properties on a `Game` object doesn't automatically update the game in Playnite's database and changes are lost with application restart. To make permanent changes game object must be updated in database manually using `Update` method from [Games](xref:Playnite.SDK.IGameDatabase.Games) collection.

**PowerShell**:

```powershell
$game = $PlayniteApi.Database.Games[SomeId]
$game.Name = "Changed Name"
$PlayniteApi.Database.Games.Update($game)
```

**IronPython**:

```python
game = PlayniteApi.Database.Games[SomeId]
game.Name = "Changed Name"
PlayniteApi.Database.Games.Update(game)
```

### Removing Games

To remove game from database use `Remove` method from [Games](xref:Playnite.SDK.IGameDatabase.Games) collection.

**PowerShell**:

```powershell
$game = $PlayniteApi.Database.Games[SomeId]
$PlayniteApi.Database.Games.Remove($game.Id)
```

**IronPython**:

```python
game = PlayniteApi.Database.Games[SomeId]
PlayniteApi.Database.Games.Remove(game.Id)
```

Handling Files
---------------------

All game related image files are stored in game database itself, with only reference Id being used on the game object itself (with exception of [BackgroundImage](xref:Playnite.SDK.Models.Game.BackgroundImage), which allows use of WEB URL as well). Following examples show how to handle game images using database API.

### Exporting Game Cover

Game cover images are referenced in [CoverImage](xref:Playnite.SDK.Models.Game.CoverImage) property. To save a file first get the file record by calling [GetFullFilePath](xref:Playnite.SDK.IGameDatabaseAPI.GetFullFilePath(System.String)) method. `GetFullFilePath` returns full path to a file on the disk drive.

**PowerShell**:

```powershell
$game = $PlayniteApi.Database.Games[SomeId]
$coverPath = $PlayniteApi.Database.GetFullFilePath($game.CoverImage)
```

**IronPython**:

```python
game = PlayniteApi.Database.Games[SomeId]
coverPath = PlayniteApi.Database.GetFullFilePath(game.CoverImage)
```

### Changing Cover Image

Changing cover image involves several steps. First remove original image by calling [RemoveFile](xref:Playnite.SDK.IGameDatabaseAPI.RemoveFile(System.String)) method. Then add new image file to a database using [AddFile](xref:Playnite.SDK.IGameDatabaseAPI.AddFile(System.String,System.Guid)). And lastly assign Id of new image to a game.

Following example changes cover image of first game in database:

**PowerShell**:

```powershell
$game = $PlayniteApi.Database.Games[SomeId]
$PlayniteApi.Database.RemoveFile($game.CoverImage)
$game.CoverImage = $PlayniteApi.Database.AddFile("c:\file.png", $game.Id)
$PlayniteApi.Database.Games.Update($game)
```

**IronPython**:

```python
game = PlayniteApi.Database.Games[SomeId]
PlayniteApi.Database.RemoveFile(game.CoverImage)
game.CoverImage = PlayniteApi.Database.AddFile("c:\\file.png", game.Id)
PlayniteApi.Database.Games.Update(game)
```