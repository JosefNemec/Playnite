using Playnite.API;
using Playnite.Common;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.ViewModels
{
    public class PluginSettingsItem
    {
        public ISettings Settings { get; set; }
        public UserControl View { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }

    public abstract class PluginSettingsHelper
    {
        private static ILogger logger = LogManager.GetLogger();

        public static UserControl GetPluginSettingsView(Guid pluginId, ExtensionFactory extensions, Dictionary<Guid, PluginSettingsItem> loadedPluginSettings)
        {
            if (loadedPluginSettings.TryGetValue(pluginId, out var settings))
            {
                return settings.View;
            }

            try
            {
                var plugin = extensions.Plugins.Values.First(a => a.Plugin.Id == pluginId);
                var provSetting = plugin.Plugin.GetSettings(false);
                var provView = plugin.Plugin.GetSettingsView(false);
                if (provSetting != null && provView != null)
                {
                    provView.DataContext = provSetting;
                    provSetting.BeginEdit();
                    var plugSetting = new PluginSettingsItem()
                    {
                        Name = plugin.Description.Name,
                        Settings = provSetting,
                        View = provView
                    };

                    loadedPluginSettings.Add(pluginId, plugSetting);
                    return provView;
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, $"Failed to load plugin settings, {pluginId}");
                return new Controls.SettingsSections.ErrorLoading();
            }

            return new Controls.SettingsSections.NoSettingsAvailable();
        }

        public static Tuple<bool, List<string>> VerifyPluginSettings(Dictionary<Guid, PluginSettingsItem> loadedPluginSettings)
        {
            foreach (var plugin in loadedPluginSettings.Values)
            {
                if (!plugin.Settings.VerifySettings(out var errors))
                {
                    logger.Error($"Plugin settings verification errors {plugin.Name}.");
                    errors?.ForEach(a => logger.Error(a));
                    if (errors == null)
                    {
                        errors = new List<string>();
                    }

                    return new Tuple<bool, List<string>>(false, errors);
                }
            }

            return new Tuple<bool, List<string>>(true, null);
        }
    }
}
