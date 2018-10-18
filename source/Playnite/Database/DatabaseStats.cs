﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Models;
using Playnite.Database;
using NLog;
using Playnite.Database.Events;

namespace Playnite.Database
{
    public class DatabaseStats : INotifyPropertyChanged, IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public int Installed { get; private set; } = 0;
        public int UnInstalled { get; private set; } = 0;
        public int Hidden { get; private set; } = 0;
        public int Favorite { get; private set; } = 0;

        public int Total
        {
            get
            {
                if (database == null)
                {
                    return 0;
                }
                else
                {
                    return database.GamesCollection.Count();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;       

        private GameDatabase database;

        public DatabaseStats(GameDatabase database)
        {
            this.database = database;
            database.GameUpdated += Database_GameUpdated;
            database.GamesCollectionChanged += Database_GamesCollectionChanged;
            database.DatabaseOpened += Database_DatabaseOpened;
            Recalculate();
        }

        private void Recalculate()
        {
            if (database.GamesCollection == null)
            {
                return;
            }

            logger.Info("Completely recalculating database statistics...");

            Installed = 0;
            UnInstalled = 0;
            Hidden = 0;
            Favorite = 0;

            foreach (var game in database.GamesCollection.FindAll().ToList())
            {
                if (game.IsInstalled)
                {
                    Installed++;
                }
                else
                {
                    UnInstalled++;
                }                

                if (game.Hidden)
                {
                    Hidden++;
                }

                if (game.Favorite)
                {
                    Favorite++;
                }
            }

            NotifiyAllChanged();
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void NotifiyAllChanged()
        {
            OnPropertyChanged("Installed");
            OnPropertyChanged("UnInstalled");
            OnPropertyChanged("Hidden");
            OnPropertyChanged("Favorite");
            OnPropertyChanged("Total");
        }

        private void Database_DatabaseOpened(object sender, EventArgs e)
        {
            Recalculate();
        }

        private void Database_GamesCollectionChanged(object sender, GamesCollectionChangedEventArgs args)
        {
            foreach (var game in args.RemovedGames)
            {
                IncrementalUpdate(game, -1);
            }

            foreach (var game in args.AddedGames)
            {
                IncrementalUpdate(game, 1);
            }

            NotifiyAllChanged();
        }

        private void Database_GameUpdated(object sender, GameUpdatedEventArgs args)
        {
            foreach (var update in args.UpdatedGames)
            {
                if (update.OldData.Hidden != update.NewData.Hidden)
                {
                    Hidden = Hidden + (1 * (update.NewData.Hidden ? 1 : -1));
                }

                if (update.OldData.Favorite != update.NewData.Favorite)
                {
                    Favorite = Favorite + (1 * (update.NewData.Favorite ? 1 : -1));
                }

                if (update.OldData.IsInstalled != update.NewData.IsInstalled)
                {
                    Installed = Installed + (1 * (update.NewData.IsInstalled ? 1 : -1));
                    UnInstalled = UnInstalled + (1 * (!update.NewData.IsInstalled ? 1 : -1));
                }
            }

            OnPropertyChanged("Installed");
            OnPropertyChanged("UnInstalled");
            OnPropertyChanged("Hidden");
            OnPropertyChanged("Favorite");
        }

        private void IncrementalUpdate(Game game, int modifier)
        {
            if (game.Hidden)
            {
                Hidden = Hidden + (1 * modifier);
            }

            if (game.Favorite)
            {
                Favorite = Favorite + (1 * modifier);
            }

            if (game.IsInstalled)
            {
                Installed = Installed + (1 * modifier);
            }
            else
            {
                UnInstalled = UnInstalled + (1 * modifier);
            }
        }

        public void Dispose()
        {
            database.DatabaseOpened -= Database_DatabaseOpened;
            database.GamesCollectionChanged -= Database_GamesCollectionChanged;
            database.GameUpdated -= Database_GameUpdated;
        }
    }
}
