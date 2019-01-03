﻿using Playnite;
using Playnite.API;
using Playnite.Common.System;
using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.Settings;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static PlayniteUI.ViewModels.InstalledGamesViewModel;

namespace PlayniteUI.ViewModels
{    
    public class FirstTimeStartupViewModel : ObservableObject
    {
        public class Pages
        {
            public const int Intro = 0;
            public const int ProviderSelect = 1;
            public const int ProviderConfig = 2;
            public const int Custom = 3;
            public const int Finish = 4;
        }

        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private ExtensionFactory extensions;
        private IPlayniteAPI playniteApi;
        private List<PluginSettings> selectedPlugins;
        private int selectedPluginIndex = 0;

        public bool ShowFinishButton
        {
            get => SelectedIndex == Pages.Finish;
        }
        
        public List<InstalledGameMetadata> ImportedGames
        {
            get;
            private set;
        } = new List<InstalledGameMetadata>();

        private PlayniteSettings settings = new PlayniteSettings();
        public PlayniteSettings Settings
        {
            get
            {
                return settings;
            }

            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                selectedIndex = value;
                OnPropertyChanged(nameof(ShowFinishButton));
                OnPropertyChanged();
            }
        }

        private UserControl selectedProviderSettingsView;
        public UserControl SelectedProviderSettingsView
        {
            get
            {
                return selectedProviderSettingsView;
            }

            set
            {
                selectedProviderSettingsView = value;
                OnPropertyChanged();
            }
        }

        private PluginSettings selectedPlugin;
        public PluginSettings SelectedLibraryPlugin
        {
            get
            {
                return selectedPlugin;
            }

            set
            {
                selectedPlugin = value;
                OnPropertyChanged();
            }
        }

        public List<SelectablePlugin> LibraryPlugins
        {
            get;
        } = new List<SelectablePlugin>();

        #region Commands

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(false);
            });
        }

        public RelayCommand<object> FinishCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(true);
            });
        }

        public RelayCommand<object> NextCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                NavigateNext();
            }, (a) => SelectedIndex < Pages.Finish);
        }

        public RelayCommand<object> BackCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                NavigateBack();
            }, (a) => SelectedIndex > 0);
        }

        public RelayCommand<object> ImportGamesCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ImportGames(new InstalledGamesViewModel(
                    InstalledGamesWindowFactory.Instance, new DialogsFactory()));
            });
        }

        #endregion Commands

        public FirstTimeStartupViewModel(
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources,
            ExtensionFactory extensions,
            IPlayniteAPI playniteApi)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.extensions = extensions;
            this.playniteApi = playniteApi;

            var plugins = extensions.GetExtensionDescriptors().Where(a => a.Type == ExtensionType.GameLibrary);
            foreach (var description in plugins)
            {
                var provider = extensions.LoadPlugin<ILibraryPlugin>(description, playniteApi);
                LibraryPlugins.Add(new SelectablePlugin(provider.IsClientInstalled, provider, description));
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            Settings.DisabledPlugins = LibraryPlugins.Where(a => !a.Selected)?.Select(a => a.Description.FolderName).ToList();
            foreach (var plugin in LibraryPlugins)
            {
                plugin.Plugin.Dispose();
            }

            window.Close(result);
        }

        private void SetPluginConfiguration(PluginSettings plugin)
        {
            SelectedLibraryPlugin = plugin;
            var view = plugin.View;
            view.DataContext = plugin.Settings;
            SelectedProviderSettingsView = view;
            plugin.Settings.BeginEdit();
        }

        public void NavigateNext()
        {
            if (SelectedIndex == Pages.ProviderSelect)
            {
                selectedPluginIndex = 0;
                selectedPlugins = LibraryPlugins.Where(a => a.Selected)?.Select(a =>
                {
                    var lib = a.Plugin as ILibraryPlugin;
                    return new PluginSettings()
                    {
                        Name = lib.Name,
                        View = lib.GetSettingsView(true),
                        Settings = lib.GetSettings(true),
                        Icon = lib.LibraryIcon
                    };
                }).Where(a => a.View != null).ToList();

                if (selectedPlugins?.Any() == true)
                {
                    SetPluginConfiguration(selectedPlugins[0]);        
                }
                else
                {
                    SelectedIndex++;
                    SelectedIndex++;
                    return;
                }
            }

            if (SelectedIndex == Pages.ProviderConfig)
            {
                if (SelectedLibraryPlugin.Settings.VerifySettings(out var errors))
                {
                    SelectedLibraryPlugin.Settings.EndEdit();
                }
                else
                {
                    dialogs.ShowErrorMessage(string.Join(Environment.NewLine, errors), "");
                    return;
                }

                if ((selectedPluginIndex + 1) < selectedPlugins.Count)
                {
                    selectedPluginIndex++;
                    SetPluginConfiguration(selectedPlugins[selectedPluginIndex]);
                    return;
                }
            }

            SelectedIndex++;
        }

        public void NavigateBack()
        {
            if (SelectedIndex == Pages.Custom)
            {
                SelectedIndex--;
            }

            SelectedIndex--;
        }       

        public void ImportGames(InstalledGamesViewModel model)
        {
            if (model.OpenView() == true)
            {
                logger.Debug($"Selected {model.Games} custom games from first time wizard.");
                ImportedGames = model.Games;
            }
        }
    }
}
