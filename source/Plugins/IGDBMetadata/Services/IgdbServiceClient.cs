using Playnite.Common;
using Playnite.SDK;
using Playnite.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IgdbModels = PlayniteServices.Models.IGDB;
using SdkModels = Playnite.SDK.Models;

namespace IGDBMetadata.Services
{
    public class IgdbServiceClient : BaseServicesClient
    {
        private readonly ILogger logger = LogManager.GetLogger();

        public IgdbServiceClient(Version playniteVersion) : base(ConfigurationManager.AppSettings["ServicesUrl"], playniteVersion)
        {
        }

        public IgdbServiceClient(Version playniteVersion, string endpoint) : base(endpoint, playniteVersion)
        {
        }

        public IgdbModels.ExpandedGame GetMetadata(SdkModels.Game game)
        {
            // Only serialize minimum amount of data needed
            var gameData = new Dictionary<string, object>
            {
                { nameof(SdkModels.Game.Name), game.Name },
                { nameof(SdkModels.Game.ReleaseDate), game.ReleaseDate },
                { nameof(SdkModels.Game.PluginId), game.PluginId },
                { nameof(SdkModels.Game.GameId), game.GameId }
            }.ToJson(false);
            return ExecutePostRequest<IgdbModels.ExpandedGame>("/igdb/metadata", gameData);
        }

        public List<IgdbModels.ExpandedGame> GetIGDBGames(string searchName)
        {
            var encoded = Uri.EscapeDataString(searchName);
            return ExecuteGetRequest<List<IgdbModels.ExpandedGame>>($"/igdb/games/{encoded}");
        }

        public IgdbModels.ExpandedGame GetIGDBGameParsed(UInt64 id, string apiKey = null)
        {
            return ExecuteGetRequest<IgdbModels.ExpandedGame>($"/igdb/game_parsed/{id}");
        }
    }
}
