def import_games_from_file():
    # Use the SelectFile dialog to prompt the user for the file with the list of
    # games separated by newline.
    filename = PlayniteApi.Dialogs.SelectFile("All files (*.*)|*.*")

    # If the user did not select a file, do not try to add any games.
    if filename == "":
        return 1

    # Assuming each line contains the name of a game to be added, read the file
    # line-by-line, create a Game object with the name, and add the game to the
    # database.
    __logger.Info("Importing games from %s" % filename)
    with open(filename) as fin:
        for game in fin:
            __logger.Info("Adding %s to the database" % game.strip())
            PlayniteApi.Database.Games.Add(Game(game.strip()))
            __logger.Info("%s has been added to the database" % game.strip())

