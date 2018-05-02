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
using Playnite.Database;
using System.IO;
using Playnite.SDK.Models;
using Playnite.SDK;

namespace Playnite.MetaProviders
{
    public class IGDBMetadataProvider : IMetadataProvider
    {
        private ServicesClient client;
        private string apiKey;

        public IGDBMetadataProvider()
        {
            client = new ServicesClient();
        }

        public IGDBMetadataProvider(string apiKey)
        {
            client = new ServicesClient();
            this.apiKey = apiKey;
        }

        public IGDBMetadataProvider(ServicesClient client)
        {
            this.client = client;
        }

        public IGDBMetadataProvider(ServicesClient client, string apiKey)
        {
            this.client = client;
            this.apiKey = apiKey;
        }

        public List<PlayniteServices.Models.IGDB.Game> Search(string game)
        {
            return client.GetIGDBGames(game, apiKey);
        }

        public Game GetParsedGame(ulong id)
        {
            var dbGame = client.GetIGDBGameParsed(id, apiKey);
            var game = new Game()
            {
                Name = dbGame.name,
                Description = dbGame.summary.Replace("\n", "\n<br>")
            };

            if (dbGame.cover != null)
            {
                game.Image = dbGame.cover.Replace("t_thumb", "t_cover_big");
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

            if (dbGame.genres?.Any() == true)
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

            if (dbGame.aggregated_rating != 0)
            {
                game.CriticScore = Convert.ToInt32(dbGame.aggregated_rating);
            }

            if (dbGame.rating != 0)
            {
                game.CommunityScore = Convert.ToInt32(dbGame.rating);
            }

            return game;
        }

        public bool GetSupportsIdSearch()
        {
            return false;
        }

        public List<MetadataSearchResult> SearchGames(string gameName)
        {
            return Search(gameName)?.Select(a => new MetadataSearchResult()
            {
                Id = a.id.ToString(),
                Name = a.name,
                ReleaseDate = a.first_release_date == 0 ? (DateTime?)null : DateTimeOffset.FromUnixTimeMilliseconds(a.first_release_date).DateTime
            }).ToList();
        }

        public GameMetadata GetGameData(string gameId)
        {
            var game = GetParsedGame(ulong.Parse(gameId));
            FileDefinition image = null;
            if (!string.IsNullOrEmpty(game.Image))
            {
                var name = Path.GetFileName(game.Image);
                image = new FileDefinition($"images/custom/{name}", name, Web.DownloadData(game.Image));
            }

            return new GameMetadata(game, null, image, string.Empty);
        }
    }
}
