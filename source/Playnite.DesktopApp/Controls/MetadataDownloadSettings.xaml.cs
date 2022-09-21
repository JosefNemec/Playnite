﻿using Playnite.Metadata;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Playnite.DesktopApp.Controls
{
    /// <summary>
    /// Interaction logic for MetadataDownloadSettings.xaml
    /// </summary>
    public partial class MetadataDownloadSettings : UserControl, INotifyPropertyChanged
    {
        public class MetadataSource : ObservableObject
        {
            private bool enabled = true;
            public bool Enabled
            {
                get => enabled;
                set
                {
                    enabled = value;
                    OnPropertyChanged();
                }
            }

            private Guid id = Guid.Empty;
            public Guid Id
            {
                get => id;
                set
                {
                    id = value;
                    OnPropertyChanged();
                }
            }

            private string name;
            public string Name
            {
                get => name;
                set
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }

        public class FieldsSelectionSettings : ObservableObject
        {
            public RelayCommand<MetadataSource> MoveSourceUpCommand
            {
                get => new RelayCommand<MetadataSource>((a) =>
                {
                    var index = Sources.IndexOf(a);
                    if (Sources.Count > 1 && (index - 1) >= 0)
                    {
                        Sources.Remove(a);
                        Sources.Insert(index - 1, a);
                    }
                });
            }

            public RelayCommand<MetadataSource> MoveSourceDownCommand
            {
                get => new RelayCommand<MetadataSource>((a) =>
                {
                    var index = Sources.IndexOf(a);
                    if (Sources.Count > 1 && (index + 1) < Sources.Count)
                    {
                        Sources.Remove(a);
                        Sources.Insert(index + 1, a);
                    }
                });
            }

            public ObservableCollection<MetadataSource> Sources
            {
                get; set;
            }

            public string SelectionText
            {
                get => string.Join(", ", Sources.Where(a => a.Enabled).Select(a => a.Name).ToArray());
            }

            public event EventHandler SettingsChanged;

            public FieldsSelectionSettings(ObservableCollection<MetadataSource> sources)
            {
                Sources = sources;
                Sources.CollectionChanged += (s, e) =>
                {
                    OnSettingsChanged();
                };

                foreach (var source in Sources)
                {
                    source.PropertyChanged += (s, e) =>
                    {
                        OnSettingsChanged();
                    };
                }
            }

            private void OnSettingsChanged()
            {
                OnPropertyChanged(nameof(SelectionText));
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        #region Setting fields

        private FieldsSelectionSettings nameSettings;
        public FieldsSelectionSettings NameSettings
        {
            get => nameSettings;
            set
            {
                nameSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings genresSettings;
        public FieldsSelectionSettings GenresSettings
        {
            get => genresSettings;
            set
            {
                genresSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings releaseDateSettings;
        public FieldsSelectionSettings ReleaseDateSettings
        {
            get => releaseDateSettings;
            set
            {
                releaseDateSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings developersSettings;
        public FieldsSelectionSettings DevelopersSettings
        {
            get => developersSettings;
            set
            {
                developersSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings publishersSettings;
        public FieldsSelectionSettings PublishersSettings
        {
            get => publishersSettings;
            set
            {
                publishersSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings tagsSettings;
        public FieldsSelectionSettings TagsSettings
        {
            get => tagsSettings;
            set
            {
                tagsSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings featuresSettings;
        public FieldsSelectionSettings FeaturesSettings
        {
            get => featuresSettings;
            set
            {
                featuresSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings descriptionSettings;
        public FieldsSelectionSettings DescriptionSettings
        {
            get => descriptionSettings;
            set
            {
                descriptionSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings linksSettings;
        public FieldsSelectionSettings LinksSettings
        {
            get => linksSettings;
            set
            {
                linksSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings communityScoreSettings;
        public FieldsSelectionSettings CommunityScoreSettings
        {
            get => communityScoreSettings;
            set
            {
                communityScoreSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings criticScoreSettings;
        public FieldsSelectionSettings CriticScoreSettings
        {
            get => criticScoreSettings;
            set
            {
                criticScoreSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings coverSettings;
        public FieldsSelectionSettings CoverSettings
        {
            get => coverSettings;
            set
            {
                coverSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings iconSettings;
        public FieldsSelectionSettings IconSettings
        {
            get => iconSettings;
            set
            {
                iconSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings backgroundImageSettings;
        public FieldsSelectionSettings BackgroundImageSettings
        {
            get => backgroundImageSettings;
            set
            {
                backgroundImageSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings ageRatingSettings;
        public FieldsSelectionSettings AgeRatingSettings
        {
            get => ageRatingSettings;
            set
            {
                ageRatingSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings seriesSettings;
        public FieldsSelectionSettings SeriesSettings
        {
            get => seriesSettings;
            set
            {
                seriesSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings regionSettings;
        public FieldsSelectionSettings RegionSettings
        {
            get => regionSettings;
            set
            {
                regionSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings platformSettings;
        public FieldsSelectionSettings PlatformSettings
        {
            get => platformSettings;
            set
            {
                platformSettings = value;
                OnPropertyChanged();
            }
        }

        private FieldsSelectionSettings installSizeSettings;
        public FieldsSelectionSettings InstallSizeSettings
        {
            get => installSizeSettings;
            set
            {
                installSizeSettings = value;
                OnPropertyChanged();
            }
        }

        #endregion Setting fields

        #region Properties

        public MetadataDownloaderSettings Settings
        {
            get
            {
                return (MetadataDownloaderSettings)GetValue(SettingsProperty);
            }

            set
            {
                SetValue(SettingsProperty, value);
            }
        }

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
            nameof(Settings),
            typeof(MetadataDownloaderSettings),
            typeof(MetadataDownloadSettings),
            new PropertyMetadata(new MetadataDownloaderSettings(), SettingsPropertyChangedCallback));

        private static void SettingsPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null || PlayniteApplication.Current == null)
            {
                return;
            }

            var control = (MetadataDownloadSettings)sender;
            var settings = e.NewValue as MetadataDownloaderSettings;
            var plugins = PlayniteApplication.Current.Extensions.MetadataPlugins;

            control.NameSettings = control.SetupField(settings.Name, MetadataField.Name, plugins);
            control.NameSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.Name.Sources = control.NameSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.GenresSettings = control.SetupField(settings.Genre, MetadataField.Genres, plugins);
            control.GenresSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.Genre.Sources = control.GenresSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.ReleaseDateSettings = control.SetupField(settings.ReleaseDate, MetadataField.ReleaseDate, plugins);
            control.ReleaseDateSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.ReleaseDate.Sources = control.ReleaseDateSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.DevelopersSettings = control.SetupField(settings.Developer, MetadataField.Developers, plugins);
            control.DevelopersSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.Developer.Sources = control.DevelopersSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.PublishersSettings = control.SetupField(settings.Publisher, MetadataField.Publishers, plugins);
            control.PublishersSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.Publisher.Sources = control.PublishersSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.TagsSettings = control.SetupField(settings.Tag, MetadataField.Tags, plugins);
            control.TagsSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.Tag.Sources = control.TagsSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.FeaturesSettings = control.SetupField(settings.Feature, MetadataField.Features, plugins);
            control.FeaturesSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.Feature.Sources = control.FeaturesSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.DescriptionSettings = control.SetupField(settings.Description, MetadataField.Description, plugins);
            control.DescriptionSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.Description.Sources = control.DescriptionSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.LinksSettings = control.SetupField(settings.Links, MetadataField.Links, plugins);
            control.LinksSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.Links.Sources = control.LinksSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.CriticScoreSettings = control.SetupField(settings.CriticScore, MetadataField.CriticScore, plugins);
            control.CriticScoreSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.CriticScore.Sources = control.CriticScoreSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.CommunityScoreSettings = control.SetupField(settings.CommunityScore, MetadataField.CommunityScore, plugins);
            control.CommunityScoreSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.CommunityScore.Sources = control.CommunityScoreSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.CoverSettings = control.SetupField(settings.CoverImage, MetadataField.CoverImage, plugins);
            control.CoverSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.CoverImage.Sources = control.CoverSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.IconSettings = control.SetupField(settings.Icon, MetadataField.Icon, plugins);
            control.IconSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.Icon.Sources = control.IconSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.BackgroundImageSettings = control.SetupField(settings.BackgroundImage, MetadataField.BackgroundImage, plugins);
            control.BackgroundImageSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.BackgroundImage.Sources = control.BackgroundImageSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.AgeRatingSettings = control.SetupField(settings.AgeRating, MetadataField.AgeRating, plugins);
            control.AgeRatingSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.AgeRating.Sources = control.AgeRatingSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.RegionSettings = control.SetupField(settings.Region, MetadataField.Region, plugins);
            control.RegionSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.Region.Sources = control.RegionSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.SeriesSettings = control.SetupField(settings.Series, MetadataField.Series, plugins);
            control.SeriesSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.Series.Sources = control.SeriesSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.PlatformSettings = control.SetupField(settings.Platform, MetadataField.Platform, plugins);
            control.PlatformSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.Platform.Sources = control.PlatformSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };

            control.InstallSizeSettings = control.SetupField(settings.InstallSize, MetadataField.InstallSize, plugins);
            control.InstallSizeSettings.SettingsChanged += (_, __) =>
            {
                control.Settings.InstallSize.Sources = control.InstallSizeSettings.Sources.Where(a => a.Enabled).Select(a => a.Id).ToList();
            };
        }

        #endregion Properties

        private FieldsSelectionSettings allSettings;
        public FieldsSelectionSettings AllSettings
        {
            get => allSettings;
            set
            {
                allSettings = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand SetAllPropertiesCommmand => new RelayCommand(() => SetAllProperties());
        public RelayCommand SelectAllToImportCommand => new RelayCommand(() => SelectAllSelectionImport(true));
        public RelayCommand DeselectAllToImportCommand => new RelayCommand(() => SelectAllSelectionImport(false));

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MetadataDownloadSettings()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            var allSources = new ObservableCollection<MetadataSource> { new MetadataSource
            {
                Id = Guid.Empty,
                Enabled = false,
                Name = ResourceProvider.GetString(LOC.MetaSourceStore)
            }};

            foreach (var plugin in PlayniteApplication.Current.Extensions.MetadataPlugins)
            {
                allSources.Add(new MetadataSource
                {
                    Id = plugin.Id,
                    Enabled = false,
                    Name = plugin.Name
                });
            }

            AllSettings = new FieldsSelectionSettings(allSources);
        }

        internal FieldsSelectionSettings SetupField(
            MetadataFieldSettings settings,
            MetadataField field,
            List<MetadataPlugin> plugins)
        {
            var sources = new ObservableCollection<MetadataSource>();
            var storeAdded = false;
            foreach (var src in settings.Sources)
            {
                if (src == Guid.Empty)
                {
                    storeAdded = true;
                    sources.Add(new MetadataSource
                    {
                        Id = Guid.Empty,
                        Enabled = true,
                        Name = ResourceProvider.GetString(LOC.MetaSourceStore)
                    });
                }
                else
                {
                    var plugin = plugins.FirstOrDefault(a => a.Id == src);
                    if (plugin?.SupportedFields.Contains(field) == true)
                    {
                        sources.Add(new MetadataSource
                        {
                            Id = plugin.Id,
                            Enabled = true,
                            Name = plugin.Name
                        });
                    }
                }
            }

            if (!storeAdded)
            {
                sources.Add(new MetadataSource
                {
                    Id = Guid.Empty,
                    Enabled = false,
                    Name = ResourceProvider.GetString("LOCMetaSourceStore")
                });
            }

            foreach (var plugin in plugins)
            {
                if (plugin.SupportedFields.Contains(field) && sources.Any(a => a.Id == plugin.Id) == false)
                {
                    sources.Add(new MetadataSource
                    {
                        Id = plugin.Id,
                        Enabled = false,
                        Name = plugin.Name
                    });
                }
            }

            return new FieldsSelectionSettings(sources);
        }

        private void SetAllProperties()
        {
            void setSources(FieldsSelectionSettings settings)
            {
                settings.Sources.ForEach(a => a.Enabled = AllSettings.Sources.FirstOrDefault(b => a.Id == b.Id)?.Enabled ?? false);
                foreach (var allSource in AllSettings.Sources.Reverse())
                {
                    var toMove = settings.Sources.FirstOrDefault(a => a.Id == allSource.Id);
                    if (toMove != null)
                    {
                        var oldIndex = settings.Sources.IndexOf(toMove);
                        if (oldIndex != 0)
                        {
                            settings.Sources.Move(oldIndex, 0);
                        }
                    }
                }
            }

            foreach (var prop in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(a => a.PropertyType == typeof(FieldsSelectionSettings) && a.Name != nameof(AllSettings)))
            {
                setSources((FieldsSelectionSettings)prop.GetValue(this));
            }
        }

        private void SelectAllSelectionImport(bool select)
        {
            foreach (var prop in Settings.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(a => a.PropertyType == typeof(MetadataFieldSettings)))
            {
                ((MetadataFieldSettings)prop.GetValue(Settings)).Import = select;
            }
        }
    }
}
