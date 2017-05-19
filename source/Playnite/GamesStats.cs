using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Models;

namespace Playnite
{
    public class GamesStats : INotifyPropertyChanged
    {
        private int installed = 0;
        public int Installed
        {
            get
            {
                return installed;
            }
        }

        private int hidden = 0;
        public int Hidden
        {
            get
            {
                return hidden;
            }
        }

        private int favorite = 0;
        public int Favorite
        {
            get
            {
                return favorite;
            }
        }

        private int origin = 0;
        public int Origin
        {
            get
            {
                return origin;
            }
        }

        private int steam = 0;
        public int Steam
        {
            get
            {
                return steam;
            }
        }

        private int gog = 0;
        public int GOG
        {
            get
            {
                return gog;
            }
        }

        private int custom = 0;
        public int Custom
        {
            get
            {
                return custom;
            }
        }

        public int Total
        {
            get
            {
                if (games != null)
                {
                    return games.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<IGame> games;

        private void Recalculate()
        {
            installed = 0;
            hidden = 0;
            favorite = 0;
            origin = 0;
            steam = 0;
            gog = 0;
            custom = 0;

            foreach (var game in games)
            {
                if (game.IsInstalled)
                {
                    installed++;
                }

                if (game.Hidden)
                {
                    hidden++;
                }

                if (game.Favorite)
                {
                    favorite++;
                }

                switch (game.Provider)
                {
                    case Provider.Custom:
                        custom++;
                        break;
                    case Provider.GOG:
                        gog++;
                        break;
                    case Provider.Origin:
                        origin++;
                        break;
                    case Provider.Steam:
                        steam++;
                        break;
                    default:
                        break;
                }
            }

            OnPropertyChanged("Installed");
            OnPropertyChanged("Hidden");
            OnPropertyChanged("Favorite");
            OnPropertyChanged("Origin");
            OnPropertyChanged("Steam");
            OnPropertyChanged("GOG");
            OnPropertyChanged("Custom");
            OnPropertyChanged("Total");
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void SetGames(ObservableCollection<IGame> gameList)
        {
            if (games != null)
            {
                games.CollectionChanged -= Games_CollectionChanged;
            }

            games = gameList;
            games.CollectionChanged += Games_CollectionChanged;

            foreach (var game in games)
            {
                game.PropertyChanged += Game_PropertyChanged;
            }

            Recalculate();
        }

        private void Game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Hidden":
                    Recalculate();
                    break;
                case "IsInstalled":
                    Recalculate();
                    break;
                case "Favorite":
                    Recalculate();
                    break;
                default:
                    break;
            }
        }

        private void Games_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (IGame newGame in e.NewItems)
                {
                    newGame.PropertyChanged += Game_PropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (IGame oldGame in e.OldItems)
                {
                    oldGame.PropertyChanged -= Game_PropertyChanged;
                }
            }

            Recalculate();
        }
    }
}
