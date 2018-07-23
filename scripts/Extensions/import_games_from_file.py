__attributes = {
    'Author': 'Michael Nguyen <hallosputnik@gmail.com>',
    'Version': '0.1.0'
}

__exports = [
    {
        'Name': 'Import Games from File',
        'Function': 'import_games_from_file'
    }
]

def import_games_from_file():
    # Use the SelectFile dialog to prompt the user for the file with the list of
    # games separated by newline.
    filename = PlayniteApi.Dialogs.SelectFile("All files (*.*)|*.*")

    # Assuming each line contains the name of a game to be added, read the file
    # line-by-line, create a Game object with the name, and add the game to the
    # database.
    with open(filename) as fin:
        for game in fin:
            PlayniteApi.Database.AddGame(Game(game.strip()))

