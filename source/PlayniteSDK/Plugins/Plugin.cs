using Playnite.SDK.Data;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Playnite.SDK.Plugins
{
    /// <summary>
    ///
    /// </summary>
    public class GetPlayActionsArgs
    {
        /// <summary>
        ///
        /// </summary>
        public Game Game { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class GetInstallActionsArgs
    {
        /// <summary>
        ///
        /// </summary>
        public Game Game { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class GetUninstallActionsArgs
    {
        /// <summary>
        ///
        /// </summary>
        public Game Game { get; set; }
    }

    /// <summary>
    /// When used, specific plugin class will be loaded by Playnite.
    /// </summary>
    public class LoadPluginAttribute : Attribute
    {
    }

    /// <summary>
    /// When used, specific plugin class won't be loaded by Playnite.
    /// </summary>
    public class IgnorePluginAttribute : Attribute
    {
    }

    /// <summary>
    ///
    /// </summary>
    public class GetGameViewControlArgs
    {
        /// <summary>
        ///
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        public ApplicationMode Mode { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class AddCustomElementSupportArgs
    {
        /// <summary>
        ///
        /// </summary>
        public List<string> ElementList { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string SourceName { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class AddSettingsSupportArgs
    {
        /// <summary>
        ///
        /// </summary>
        public string SourceName { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string SettingsRoot { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public class AddConvertersSupportArgs
    {
        /// <summary>
        ///
        /// </summary>
        public List<IValueConverter> Converters { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string SourceName { get; set; }
    }

    /// <summary>
    /// Represents plugin properties.
    /// </summary>
    public abstract class PluginProperties
    {
        /// <summary>
        /// Gets or sets value indicating that the plugin provides user settings view.
        /// </summary>
        public bool HasSettings { get; set; }
    }

    /// <summary>
    /// Represents <see cref="GenericPlugin"/> plugin properties.
    /// </summary>
    public class GenericPluginProperties : PluginProperties
    {
    }

    /// <summary>
    /// Represents generic plugin.
    /// </summary>
    public abstract class GenericPlugin : Plugin
    {
        /// <summary>
        /// Gets plugin's properties.
        /// </summary>
        public GenericPluginProperties Properties { get; protected set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="playniteAPI"></param>
        public GenericPlugin(IPlayniteAPI playniteAPI) : base(playniteAPI)
        {
        }
    }

    /// <summary>
    /// Represents base Playnite plugin.
    /// </summary>
    public abstract class Plugin : IDisposable, IIdentifiable
    {
        private const string pluginSettingFileName = "config.json";

        /// <summary>
        /// Gets or sets list of global searches.
        /// </summary>
        public List<SearchSupport> Searches { get; set; }

        /// <summary>
        /// Gets instance of runtime <see cref="IPlayniteAPI"/>.
        /// </summary>
        public readonly IPlayniteAPI PlayniteApi;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public abstract Guid Id { get; }

        /// <summary>
        /// Creates new instance of <see cref="Plugin"/>.
        /// </summary>
        /// <param name="playniteAPI">Instance of Playnite API to be injected.</param>
        public Plugin(IPlayniteAPI playniteAPI)
        {
            PlayniteApi = playniteAPI;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Gets plugin settings or null if plugin doesn't provide any settings.
        /// </summary>
        public virtual ISettings GetSettings(bool firstRunSettings)
        {
            return null;
        }

        /// <summary>
        /// Gets plugin settings view or null if plugin doesn't provide settings view.
        /// </summary>
        public virtual UserControl GetSettingsView(bool firstRunView)
        {
            return null;
        }

        /// <summary>
        /// Called before game is started.
        /// </summary>
        public virtual void OnGameStarting(OnGameStartingEventArgs args)
        {
        }

        /// <summary>
        /// Called when game has started.
        /// </summary>
        public virtual void OnGameStarted(OnGameStartedEventArgs args)
        {
        }

        /// <summary>
        /// Called when game stopped running.
        /// </summary>
        public virtual void OnGameStopped(OnGameStoppedEventArgs args)
        {
        }

        /// <summary>
        /// Called when game startup is cancelled.
        /// </summary>
        public virtual void OnGameStartupCancelled(OnGameStartupCancelledEventArgs args)
        {
        }

        /// <summary>
        /// Called when game has been installed.
        /// </summary>
        public virtual void OnGameInstalled(OnGameInstalledEventArgs args)
        {
        }

        /// <summary>
        /// Called when game has been uninstalled.
        /// </summary>
        public virtual void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
        }

        /// <summary>
        /// Called when game selection changed.
        /// </summary>
        public virtual void OnGameSelected(OnGameSelectedEventArgs args)
        {
        }

        /// <summary>
        /// Called when appliaction is started and initialized.
        /// </summary>
        public virtual void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
        }

        /// <summary>
        /// Called when appliaction is shutting down.
        /// </summary>
        public virtual void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
        }

        /// <summary>
        /// Called when library update has been finished.
        /// </summary>
        public virtual void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
        }

        /// <summary>
        /// Gets list of items to be displayed in game's context menu.
        /// </summary>
        /// <param name="args">Contextual arguments.</param>
        /// <returns>List of menu items to be displayed in game menu.</returns>
        public virtual IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return null;
        }

        /// <summary>
        /// Gets list of items to be displayed in Playnite's main menu.
        /// </summary>
        /// <param name="args">Contextual arguments.</param>
        /// <returns>List of menu items to be displayed in Playnite's main menu.</returns>
        public virtual IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return null;
        }

        /// <summary>
        /// Gets path dedicated for plugins to store data.
        /// </summary>
        /// <returns>Full directory path.</returns>
        public string GetPluginUserDataPath()
        {
            var path = Path.Combine(PlayniteApi.Paths.ExtensionsDataPath, Id.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// Gets plugin configuration stored in plugin.cfg file.
        /// </summary>
        /// <typeparam name="TConfig">Plugin configuration type.</typeparam>
        /// <returns>Plugin configuration.</returns>
        public TConfig GetPluginConfiguration<TConfig>() where TConfig : class
        {
            var pluginDir = Path.GetDirectoryName(GetType().Assembly.Location);
            var pluginConfig = Path.Combine(pluginDir, "plugin.cfg");
            if (File.Exists(pluginConfig))
            {
                return Serialization.FromJsonFile<TConfig>(pluginConfig);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets plugin settings.
        /// </summary>
        /// <typeparam name="TSettings">Plugin settings type.</typeparam>
        /// <returns>Plugin settings.</returns>
        public TSettings LoadPluginSettings<TSettings>() where TSettings : class
        {
            var setFile = Path.Combine(GetPluginUserDataPath(), pluginSettingFileName);
            if (File.Exists(setFile))
            {
                return Serialization.FromJsonFile<TSettings>(setFile);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Saves plugin settings.
        /// </summary>
        /// <typeparam name="TSettings">Plugin settings type.</typeparam>
        /// <param name="settings">Source plugin.</param>
        public void SavePluginSettings<TSettings>(TSettings settings) where TSettings : class
        {
            var setDir = GetPluginUserDataPath();
            var setFile = Path.Combine(setDir, pluginSettingFileName);
            if (!Directory.Exists(setDir))
            {
                Directory.CreateDirectory(setDir);
            }

            var strConf = Serialization.ToJson(settings, true);
            File.WriteAllText(setFile, strConf);
        }

        /// <summary>
        /// Opens plugin's settings view. Only works in Desktop application mode!
        /// </summary>
        /// <returns>True if user saved any changes, False if dialog was canceled.</returns>
        public bool OpenSettingsView()
        {
            if (PlayniteApi.ApplicationInfo.Mode == ApplicationMode.Fullscreen)
            {
                return false;
            }

            return PlayniteApi.MainView.OpenPluginSettings(Id);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual IEnumerable<PlayController> GetPlayActions(GetPlayActionsArgs args)
        {
            yield break;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args)
        {
            yield break;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual IEnumerable<UninstallController> GetUninstallActions(GetUninstallActionsArgs args)
        {
            yield break;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual Control GetGameViewControl(GetGameViewControlArgs args)
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        public void AddCustomElementSupport(AddCustomElementSupportArgs args)
        {
            PlayniteApi.AddCustomElementSupport(this, args);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        public void AddSettingsSupport(AddSettingsSupportArgs args)
        {
            PlayniteApi.AddSettingsSupport(this, args);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="args"></param>
        public void AddConvertersSupport(AddConvertersSupportArgs args)
        {
            PlayniteApi.AddConvertersSupport(this, args);
        }

        /// <summary>
        /// Gets sidebar items provided by this plugin.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<SidebarItem> GetSidebarItems()
        {
            yield break;
        }

        /// <summary>
        /// Gets top panel items provided by this plugin.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<TopPanelItem> GetTopPanelItems()
        {
            yield break;
        }

        /// <summary>
        /// Gets items to be included in default global search.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<SearchItem> GetSearchGlobalCommands()
        {
            yield break;
        }
    }
}
