using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Services;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Playnite.Models;
using System.Globalization;

namespace Playnite.MetaProviders
{
    public class IGDB
    {
        private ServicesClient client = new ServicesClient();

        public IGDB()
        {
            client = new ServicesClient();
        }

        public List<PlayniteServices.Models.IGDB.Game> Search(string game)
        {
            return client.GetIGDBGames(game);
        }

        public Game GetParsedGame(UInt64 id)
        {
            var dbGame = client.GetIGDBGameParsed(id);
            var game = new Game()
            {
                Name = dbGame.name,
                Description = dbGame.summary
            };

            if (dbGame.cover != null)
            {
                game.Image = dbGame.cover.url.Replace("t_thumb", "t_cover_big");
                if (!game.Image.StartsWith("https:", StringComparison.InvariantCultureIgnoreCase))
                {
                    game.Image = "https:" + game.Image;
                }
            }

            if (dbGame.first_release_date != 0)
            {
                game.ReleaseDate = DateTimeOffset.FromUnixTimeMilliseconds(dbGame.first_release_date).DateTime;
            }

            if (dbGame.developers?.Any() == true)
            {
                game.Developers = new ComparableList<string>(dbGame.developers);
            }

            if (dbGame.publishers?.Any() == true)
            {
                game.Publishers = new ComparableList<string>(dbGame.publishers);
            }

            if (dbGame.developers?.Any() == true)
            {
                game.Genres = new ComparableList<string>(dbGame.genres);
            }

            if (dbGame.websites?.Any() == true)
            {
                game.Links = new ObservableCollection<Link>(dbGame.websites.Select(a => new Link(a.category.ToString(), a.url)));
            }

            if (dbGame.game_modes?.Any() == true)
            {
                var cultInfo = new CultureInfo("en-US", false).TextInfo;
                game.Tags = new ComparableList<string>(dbGame.game_modes.Select(a => cultInfo.ToTitleCase(a)));
            }

            return game;
        }
    }
}
