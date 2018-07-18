using Playnite.SDK;
using Playnite.SDK.Plugins;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite.SteamLibrary
{
    public class SteamLibrary : IGameLibrary
    {
        private ILogger logger;
        private readonly IPlayniteAPI playniteApi;

        public SteamLibrary(IPlayniteAPI api)
        {
            playniteApi = api;
            logger = playniteApi.CreateLogger("SteamLibrary");
            var configPath = Path.Combine(api.GetPluginConfigPath(this), "config.json");
            Settings = new SteamLibrarySettings(configPath)
            {
                SteamUsers = GetSteamUsers()
            };
        }

        public void Dispose()
        {

        }

        public List<LocalSteamUser> GetSteamUsers()
        {
            var users = new List<LocalSteamUser>();
            if (File.Exists(Steam.LoginUsersPath))
            {
                var config = new KeyValue();

                try
                {
                    config.ReadFileAsText(Steam.LoginUsersPath);
                    foreach (var user in config.Children)
                    {
                        users.Add(new LocalSteamUser()
                        {
                            Id = ulong.Parse(user.Name),
                            AccountName = user["AccountName"].Value,
                            PersonaName = user["PersonaName"].Value,
                            Recent = user["mostrecent"].AsBoolean()
                        });
                    }
                }
                catch (Exception e) when (!Environment.IsDebugBuild)
                {
                    logger.Error(e, "Failed to get list of local users.");
                }
            }

            return users;
        }

        #region IGameLibrary

        public Guid Id { get; } = Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB");

        public string Name { get; } = "Steam";

        public IEditableObject Settings { get; private set; }

        public UserControl SettingsView
        {
            get => new SteamLibrarySettingsView();
        }

        #endregion IGameLibrary
    }
}
