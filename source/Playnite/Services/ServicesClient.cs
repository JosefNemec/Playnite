using Microsoft.Win32;
using Newtonsoft.Json;
using Playnite.App;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Services
{
    public class ServicesClient : BaseServicesClient
    {
        private static ILogger logger = LogManager.GetLogger();

        public ServicesClient() : this(ConfigurationManager.AppSettings["ServicesUrl"])
        {
        }

        public ServicesClient(string endpoint) : base(endpoint)
        {
        }

        public List<PlayniteServices.Models.IGDB.Game> GetIGDBGames(string searchName, string apiKey = null)
        {
            var encoded = Uri.EscapeDataString(searchName);
            var url = string.IsNullOrEmpty(apiKey) ? $"/igdb/games/{encoded}" : $"/igdb/games/{encoded}?apikey={apiKey}";
            return ExecuteGetRequest<List<PlayniteServices.Models.IGDB.Game>>(url);
        }

        public ulong GetIGDBGameBySteamId(string id, string apiKey = null)
        {
            var url = $"/igdb/gamesBySteamId/{id}";
            return ExecuteGetRequest<ulong>(url);
        }

        public PlayniteServices.Models.IGDB.Game GetIGDBGame(UInt64 id, string apiKey = null)
        {
            var url = string.IsNullOrEmpty(apiKey) ? $"/igdb/game/{id}" : $"/igdb/game/{id}?apikey={apiKey}";
            return ExecuteGetRequest<PlayniteServices.Models.IGDB.Game>(url);
        }

        public PlayniteServices.Models.IGDB.ParsedGame GetIGDBGameParsed(UInt64 id, string apiKey = null)
        {
            var url = string.IsNullOrEmpty(apiKey) ? $"/igdb/game_parsed/{id}" : $"/igdb/game_parsed/{id}?apikey={apiKey}";
            return ExecuteGetRequest<PlayniteServices.Models.IGDB.ParsedGame>(url);
        }

        public PlayniteServices.Models.IGDB.Company GetIGDBCompany(UInt64 id, string apiKey = null)
        {
            var url = string.IsNullOrEmpty(apiKey) ? $"/igdb/company/{id}" : $"/igdb/company/{id}?apikey={apiKey}";
            return ExecuteGetRequest<PlayniteServices.Models.IGDB.Company>(url);
        }

        public PlayniteServices.Models.IGDB.Genre GetIGDBGenre(UInt64 id, string apiKey = null)
        {
            var url = string.IsNullOrEmpty(apiKey) ? $"/igdb/genre/{id}" : $"/igdb/genre/{id}?apikey={apiKey}";
            return ExecuteGetRequest<PlayniteServices.Models.IGDB.Genre>(url);
        }

        public List<string> GetPatrons()
        {
            return ExecuteGetRequest<List<string>>("/patreon/patrons");
        }

        public void PostUserUsage(string instId)
        {
            var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var winId = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false).GetValue("ProductId").ToString().GetSHA256Hash();
            var user = new PlayniteServices.Models.Playnite.User()
            {
                Id = winId,
                WinVersion = Environment.OSVersion.VersionString,
                PlayniteVersion = Updater.GetCurrentVersion().ToString(),
                Is64Bit = Environment.Is64BitOperatingSystem
            };

            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            HttpClient.PostAsync(Endpoint + "/playnite/users", content).Wait();
        }

        public Guid UploadDiagPackage(string diagPath)
        {
            using (var fs = new FileStream(diagPath, FileMode.Open))
            {
                using (var content = new StreamContent(fs))
                {
                    var response = HttpClient.PostAsync(Endpoint + "/playnite/diag", content).GetAwaiter().GetResult();
                    var strResult = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var result = JsonConvert.DeserializeObject<ServicesResponse<Guid>>(strResult);
                    if (!string.IsNullOrEmpty(result.Error))
                    {
                        logger.Error("Service request error by proxy: " + result.Error);
                        throw new Exception(result.Error);
                    }

                    return result.Data;
                }
            }
        }
    }
}
