using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Services;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Playnite.Models;

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

        public Models.Game GetParsedGame(UInt64 id)
        {
            var dbGame = client.GetIGDBGame(id);
            var game = new Models.Game()
            {
                Name = dbGame.name,
                Description = dbGame.summary
            };

            if (dbGame.cover != null)
            {
                game.Image = "https:" + dbGame.cover.url.Replace("t_thumb", "t_cover_big");
            }

            if (dbGame.first_release_date != 0)
            {
                game.ReleaseDate = DateTimeOffset.FromUnixTimeMilliseconds(dbGame.first_release_date).DateTime;
            }

            if (dbGame.developers != null && dbGame.developers.Count > 0)
            {
                game.Developers = new List<string>();
                foreach (var developer in dbGame.developers)
                {
                    game.Developers.Add(client.GetIGDBCompany(developer).name);
                }
            }

            if (dbGame.publishers != null && dbGame.publishers.Count > 0)
            {
                game.Publishers = new List<string>();
                foreach (var publisher in dbGame.publishers)
                {
                    game.Publishers.Add(client.GetIGDBCompany(publisher).name);
                }
            }

            if (dbGame.developers != null && dbGame.developers.Count > 0)
            {
                game.Genres = new List<string>();
                foreach (var genre in dbGame.genres)
                {
                    game.Genres.Add(client.GetIGDBGenre(genre).name);
                }
            }

            if (dbGame.websites != null && dbGame.websites.Count > 0)
            {
                var links = new ObservableCollection<Link>();

                foreach (var website in dbGame.websites)
                {
                    links.Add(new Link(website.category.ToString(), website.url));
                }

                game.Links = links;
            }

            return game;
        }
    }
}
