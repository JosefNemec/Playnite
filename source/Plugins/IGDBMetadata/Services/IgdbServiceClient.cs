using Playnite.SDK;
using Playnite.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGDBMetadata.Services
{
    public class IgdbServiceClient : BaseServicesClient
    {
        private readonly ILogger logger = LogManager.GetLogger();

        public IgdbServiceClient(Version playniteVersion) : base(ConfigurationManager.AppSettings["ServicesUrl"], playniteVersion)
        {
        }

        public List<PlayniteServices.Models.IGDB.ExpandedGame> GetIGDBGames(string searchName, string apiKey = null)
        {
            var encoded = Uri.EscapeDataString(searchName);
            var url = string.IsNullOrEmpty(apiKey) ? $"/igdb/games/{encoded}" : $"/igdb/games/{encoded}?apikey={apiKey}";
            return ExecuteGetRequest<List<PlayniteServices.Models.IGDB.ExpandedGame>>(url);
        }

        public ulong GetIGDBGameBySteamId(string id, string apiKey = null)
        {
            var url = $"/igdb/gamesBySteamId/{id}";
            return ExecuteGetRequest<ulong>(url);
        }

        public PlayniteServices.Models.IGDB.ExpandedGame GetIGDBGameParsed(UInt64 id, string apiKey = null)
        {
            var url = string.IsNullOrEmpty(apiKey) ? $"/igdb/game_parsed/{id}" : $"/igdb/game_parsed/{id}?apikey={apiKey}";
            return ExecuteGetRequest<PlayniteServices.Models.IGDB.ExpandedGame>(url);
        }
    }
}
