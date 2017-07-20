using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Models;
using Playnite.Database;
using NLog;

namespace Playnite
{
    public class GamesStats : INotifyPropertyChanged
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public int Installed { get; private set; } = 0;
        public int Hidden { get; private set; } = 0;
        public int Favorite { get; private set; } = 0;
        public int Origin { get; private set; } = 0;
        public int Steam { get; private set; } = 0;
        public int GOG { get; private set; } = 0;
        public int Custom { get; private set; } = 0;

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

        public GamesStats(GameDatabase database)
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
            Hidden = 0;
            Favorite = 0;
            Origin = 0;
            Steam = 0;
            GOG = 0;
            Custom = 0;

            foreach (var game in database.GamesCollection.FindAll())
            {
                if (game.IsInstalled)
                {
                    Installed++;
                }

                if (game.Hidden)
                {
                    Hidden++;
                }

                if (game.Favorite)
                {
                    Favorite++;
                }

                switch (game.Provider)
                {
                    case Provider.Custom:
                        Custom++;
                        break;
                    case Provider.GOG:
                        GOG++;
                        break;
                    case Provider.Origin:
                        Origin++;
                        break;
                    case Provider.Steam:
                        Steam++;
                        break;
                    default:
                        break;
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
            OnPropertyChanged("Hidden");
            OnPropertyChanged("Favorite");
            OnPropertyChanged("Origin");
            OnPropertyChanged("Steam");
            OnPropertyChanged("GOG");
            OnPropertyChanged("Custom");
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
            if (args.OldData.Hidden != args.NewData.Hidden)
            {
                Hidden = Hidden + (1 * (args.NewData.Hidden ? 1 : -1));
            }

            if (args.OldData.Favorite != args.NewData.Favorite)
            {
                Favorite = Favorite + (1 * (args.NewData.Favorite ? 1 : -1));
            }

            if (args.OldData.IsInstalled != args.NewData.IsInstalled)
            {
                Installed = Installed + (1 * (args.NewData.IsInstalled ? 1 : -1));
            }

            OnPropertyChanged("Installed");
            OnPropertyChanged("Hidden");
            OnPropertyChanged("Favorite");
        }

        private void IncrementalUpdate(IGame game, int modifier)
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

            switch (game.Provider)
            {
                case Provider.Custom:
                    Custom = Custom + (1 * modifier);
                    break;
                case Provider.GOG:
                    GOG = GOG + (1 * modifier);
                    break;
                case Provider.Origin:
                    Origin = Origin + (1 * modifier);
                    break;
                case Provider.Steam:
                    Steam = Steam + (1 * modifier);
                    break;
            }
        }
    }
}
