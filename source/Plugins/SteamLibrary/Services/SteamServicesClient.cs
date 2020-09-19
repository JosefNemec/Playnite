using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.Services;
using SteamLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary.Services
{
    public class SteamServicesClient : BaseServicesClient
    {
        private readonly ILogger logger = LogManager.GetLogger();

        public SteamServicesClient(string endpoint, Version playniteVersion) : base(endpoint, playniteVersion)
        {
        }

        public List<GetOwnedGamesResult.Game> GetSteamLibrary(ulong userName, bool freeSub = false)
        {
            var url = "/steam/library/" + userName;
            if (freeSub)
            {
                url += "?freeSub=true";
            }

            return ExecuteGetRequest<List<GetOwnedGamesResult.Game>>(url);
        }
    }
}
