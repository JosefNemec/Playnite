__attributes = {
    'Author': 'Josef Nemec',
    'Version': '1.0'
}

__exports = [
    {
        'Name': 'Python Function',
        'Function' : 'menu_function'
    }
]

def on_script_loaded():    
    __logger.Info('FullPythonScript on_script_loaded')

def on_game_starting(game):
    __logger.Info('FullPythonScript on_game_starting ' + game.Name)

def on_game_started(game):
    __logger.Info('FullPythonScript on_game_started ' + game.Name)

def on_game_stopped(game, ellapsed_seconds):
    __logger.Info('FullPythonScript on_game_stopped ' + game.Name)

def on_game_installed(game):
    __logger.Info('FullPythonScript on_game_installed ' + game.Name)

def on_game_uninstalled(game):
    __logger.Info('FullPythonScript on_game_uninstalled ' + game.Name)

def menu_function():
    __logger.Info('FullPythonScript menu_function')