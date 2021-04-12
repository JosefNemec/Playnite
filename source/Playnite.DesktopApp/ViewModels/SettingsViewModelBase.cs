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

    public abstract class SettingsViewModelBase : ObservableObject
    {
        internal static ILogger logger = LogManager.GetLogger();
        internal IWindowFactory window;
        internal IDialogsFactory dialogs;
        internal IResourceProvider resources;
        internal GameDatabase database;
        internal PlayniteApplication application;
        internal PlayniteSettings originalSettings;
        internal Dictionary<Guid, PluginSettingsItem> loadedPluginSettings = new Dictionary<Guid, PluginSettingsItem>();
        internal bool closingHanled = false;
        internal List<string> editedFields = new List<string>();

        public ExtensionFactory Extensions { get; set; }

        private PlayniteSettings settings;
        public PlayniteSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private UserControl selectedSectionView;
        public UserControl SelectedSectionView
        {
            get => selectedSectionView;
            set
            {
                selectedSectionView = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView();
            });
        }

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });
        }

        public RelayCommand<object> WindowClosingCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                WindowClosing();
            });
        }

        public ObservableCollection<ImportExclusionItem> ImportExclusionList { get; }

        private readonly List<ImportExclusionItem> removedExclusionItems = new List<ImportExclusionItem>();
        public RelayCommand<IList<object>> RemoveImportExclusionItemCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (ImportExclusionItem item in items.ToList())
                {
                    ImportExclusionList.Remove(item);
                    removedExclusionItems.Add(item);
                }
            }, (items) => items != null && items.Count > 0);
        }

        public SettingsViewModelBase(
            GameDatabase database,
            PlayniteSettings settings,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            PlayniteApplication app)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.application = app;
            Extensions = extensions;
            originalSettings = settings;

            Settings = settings.GetClone();
            Settings.PropertyChanged += (s, e) => editedFields.AddMissing(e.PropertyName);
            ImportExclusionList = new ObservableCollection<ImportExclusionItem>(database.ImportExclusions.OrderBy(a => a.Name));
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public abstract void ConfirmDialog();
        public abstract void WindowClosing();
        public abstract void CloseView();

        internal UserControl GetPluginSettingsView(Guid pluginId)
        {
            if (loadedPluginSettings.TryGetValue(pluginId, out var settings))
            {
                return settings.View;
            }

            try
            {
                var plugin = Extensions.Plugins.Values.First(a => a.Plugin.Id == pluginId);
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

        internal bool VerifyPluginSettings()
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

                    dialogs.ShowErrorMessage(string.Join(Environment.NewLine, errors), plugin.Name);
                    return false;
                }
            }

            return true;
        }

        public void EndEdit()
        {
            Settings.CopyProperties(originalSettings, true, new List<string>()
            {
                nameof(PlayniteSettings.FilterSettings),
                nameof(PlayniteSettings.ViewSettings),
                nameof(PlayniteSettings.InstallInstanceId),
                nameof(PlayniteSettings.GridItemHeight),
                nameof(PlayniteSettings.WindowPositions),
                nameof(PlayniteSettings.Fullscreen)
            }, true);

            database.ImportExclusions.Remove(removedExclusionItems);
        }
    }
}
