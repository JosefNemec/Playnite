using Microsoft.Win32;
using Newtonsoft.Json;
using Playnite.Common;
using Playnite.SDK;
using Playnite.Services.Models;
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

        public ServicesClient(string endpoint) : base(endpoint, Updater.GetCurrentVersion())
        {
        }

        public List<string> GetPatrons()
        {
            return ExecuteGetRequest<List<string>>("/patreon/patrons");
        }

        public void PostUserUsage(string instId)
        {
            var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var winId = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false).GetValue("ProductId").ToString().GetSHA256Hash();
            var user = new User()
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

        public IEnumerable<AddonManifest> GetAllAddons(AddonType type, string searchTerm)
        {
            foreach (var file in Directory.GetFiles(@"e:\Devel\PlayniteAddonDatabase\addons\extensions\", "*.yaml"))
            {
                yield return Serialization.FromYamlFile<AddonManifest>(file);
            }

            foreach (var file in Directory.GetFiles(@"e:\Devel\PlayniteAddonDatabase\addons\themes\", "*.yaml"))
            {
                yield return Serialization.FromYamlFile<AddonManifest>(file);
            }
        }

        public AddonManifest GetAddon(string addonId)
        {
            foreach (var file in Directory.GetFiles(@"e:\Devel\PlayniteAddonDatabase\addons\extensions\", "*.yaml"))
            {
                var addon = Serialization.FromYamlFile<AddonManifest>(file);
                if (addon.AddonId == addonId)
                {
                    return addon;
                }
            }

            foreach (var file in Directory.GetFiles(@"e:\Devel\PlayniteAddonDatabase\addons\themes\", "*.yaml"))
            {
                var addon = Serialization.FromYamlFile<AddonManifest>(file);
                if (addon.AddonId == addonId)
                {
                    return addon;
                }
            }

            return null;
        }
    }
}
