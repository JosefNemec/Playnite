Working with Game Database
=====================

Introduction
---------------------
Game database API allows you to access and modify game library and its objects (including platforms and emulators). Database API can be accessed via `PlayniteAPI.Database` property, which provides [IDatabaseAPI](xref:Playnite.SDK.IGameDatabaseAPI) interface.

Handling Games
---------------------

### Getting Games

You can get all games stored in database by calling [GetGames](xref:Playnite.SDK.IGameDatabaseAPI.GetGames) method or getting specific game by its Id via [GetGame](xref:Playnite.SDK.IGameDatabaseAPI.GetGame(System.Int32)) method.

**PowerShell**:

```powershell
# Get all games
$games = $PlayniteApi.Database.GetGames()
# Get game with known Id
$game = $PlayniteApi.Database.GetGame(20)
```

**IronPython**:

```python
# Get all games
games = PlayniteApi.Database.GetGames()
# Get game with known Id
game = PlayniteApi.Database.GetGame(20)
```

### Adding New Game

To add a new game create new instance of [Game](xref:Playnite.SDK.Models.Game) class and call [AddGame](xref:Playnite.SDK.IGameDatabaseAPI.AddGame(Playnite.SDK.Models.Game)) method.

**PowerShell**:

```powershell
$newGame = New-Object "Playnite.SDK.Models.Game" "New Game"
$PlayniteApi.Database.AddGame($newGame)
```

**IronPython**:

```python
new_game = Game("New Game")
PlayniteApi.Database.AddGame(new_game)
```

### Changing Game Data

Changing properties on a `Game` object doesn't automatically update the game in Playnite's database and changes are lost with application restart. To make permanent changes game object must be updated in database manually using [UpdateGame](xref:Playnite.SDK.IGameDatabaseAPI.UpdateGame(Playnite.SDK.Models.Game)) method.

Following example changes name of first game in database:

**PowerShell**:

```powershell
$game = $PlayniteApi.Database.GetGames()[0]
$game.Name = "Changed Name"
$PlayniteApi.Database.UpdateGame($game)
```

**IronPython**:

```python
game = PlayniteApi.Database.GetGames()[0]
game.Name = "Changed Name"
PlayniteApi.Database.UpdateGame(game)
```

### Removing Games

To remove game from database use [RemoveGame](xref:Playnite.SDK.IGameDatabaseAPI.RemoveGame(System.Int32)) method with an game Id as parameter.

Following example removes first game from database:

**PowerShell**:

```powershell
$game = $PlayniteApi.Database.GetGames()[0]
$PlayniteApi.Database.RemoveGame($game.Id)
```

**IronPython**:

```python
game = PlayniteApi.Database.GetGames()[0]
PlayniteApi.Database.RemoveGame(game.Id)
```

Handling Files
---------------------

All game related image files are stored in game database itself, with only reference Id being used on the game object itself (with exception of [BackgroundImage](xref:Playnite.SDK.Models.Game.BackgroundImage), which allows use of WEB URL as well). Following examples show how to handle game images using database API.

### Exporting Game Cover

Game cover images are referenced in [Image](xref:Playnite.SDK.Models.Game.Image) property. To save a file first get the file record by calling [GetFile](xref:Playnite.SDK.IGameDatabaseAPI.GetFile(System.String)) method. `GetFile` returns [DatabaseFile](xref:Playnite.SDK.Models.DatabaseFile), which contains additional information like file name, size and others. To export file call [SaveFile](xref:Playnite.SDK.IGameDatabaseAPI.SaveFile(System.String,System.String)) method.

Following example exports cover image of the first game in database:

**PowerShell**:

```powershell
$game = $PlayniteApi.Database.GetGames()[0]
$cover = $PlayniteApi.Database.GetFile($game.Image)
$PlayniteApi.Database.SaveFile($cover.Id, $cover.Filename)
```

**IronPython**:

```python
game = PlayniteApi.Database.GetGames()[0]
cover = PlayniteApi.Database.GetFile(game.Image)
PlayniteApi.Database.SaveFile(cover.Id, cover.Filename)
```

### Changing Cover Image

Changing cover image involves several steps. First remove original image by calling [RemoveImage](xref:Playnite.SDK.IGameDatabaseAPI.RemoveImage(System.String,Playnite.SDK.Models.Game)) method. Then add new image file to a database using [AddFile](xref:Playnite.SDK.IGameDatabaseAPI.AddFile(System.String,System.String)). And lastly assign Id of new image to a game.

> [!NOTE] 
> [RemoveImage](xref:Playnite.SDK.IGameDatabaseAPI.RemoveImage(System.String,Playnite.SDK.Models.Game)) method will not remove image file if any other game is using it. No error is reported in that case since it's intended behavior. Similarly [AddFile](xref:Playnite.SDK.IGameDatabaseAPI.AddFile(System.String,System.String)) doesn't add new file to database if file with the same content already exits and instead returns Id of existing file.

Following example changes cover image of first game in database:

**PowerShell**:

```powershell
$game = $PlayniteApi.Database.GetGames()[0]
$PlayniteApi.Database.RemoveImage($game.Image, $game)
$fileId = $PlayniteApi.Database.AddFile("new_file_id", "c:\file.png")
$game.Image = $fileId
$PlayniteApi.Database.UpdateGame($game)
```

**IronPython**:

```python
game = PlayniteApi.Database.GetGames()[0]
PlayniteApi.Database.RemoveImage(game.Image, game)
file_id = PlayniteApi.Database.AddFile("new_file_id", "c:\\file.png")
game.Image = file_id
PlayniteApi.Database.UpdateGame(game)
```