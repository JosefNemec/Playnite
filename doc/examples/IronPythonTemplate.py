__attributes = {
    'Author': '',
    'Version': ''
}

__exports = [
    {
        'Name': 'Display Game Count',
        'Function' : 'display_game_count'
    }
]

def on_script_loaded():
    pass

def on_game_started(game):
    with open("RunningGame.txt", "w") as text_file:
        text_file.write(game.Name)

def on_game_stopped(game, ellapsed_seconds):
    with open("StoppedGame.txt", "w") as text_file:
        text_file.write("{0} was running for {1} seconds".format(game.Name, ellapsed_seconds))

def on_game_installed(game):
    pass

def on_game_uninstalled(game):
    pass

def display_game_count():
    game_count = PlayniteApi.Database.GetGames().Count
    PlayniteApi.Dialogs.ShowMessage(str(game_count))
    __logger.Info('This is message with Info severity')
    __logger.Error('This is message with Error severity')
    __logger.Debug('This is message with Debug severity')
    __logger.Warn('This is message with Warning severity')

    game = PlayniteApi.Database.GetGame(20)
    __logger.Error(game.Name)
