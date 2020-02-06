using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Metadata
{
    public enum MetadataGamesSource
    {
        AllFromDB,
        Selected,
        Filtered
    }

    public class MetadataFieldSettings : ObservableObject
    {
        private bool import = true;
        public bool Import
        {
            get => import;
            set
            {
                import = value;
                OnPropertyChanged();
            }
        }

        private List<Guid> sources = new List<Guid>();
        public List<Guid> Sources
        {
            get => sources;
            set
            {
                sources = value;
                OnPropertyChanged();
            }
        }

        public MetadataFieldSettings()
        {
        }

        public MetadataFieldSettings(bool import, List<Guid> sources)
        {
            Import = import;
            if (sources.HasItems())
            {
                Sources = sources.ToList();
            }
        }
    }

    public class MetadataDownloaderSettings : ObservableObject
    {
        private MetadataGamesSource gamesSource = MetadataGamesSource.AllFromDB;
        [JsonIgnore]
        public MetadataGamesSource GamesSource
        {
            get
            {
                return gamesSource;
            }

            set
            {
                gamesSource = value;
                OnPropertyChanged();
            }
        }

        private bool skipExistingValues = true;
        [JsonIgnore]
        public bool SkipExistingValues
        {
            get
            {
                return skipExistingValues;
            }

            set
            {
                skipExistingValues = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings name = new MetadataFieldSettings();
        public MetadataFieldSettings Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings genre = new MetadataFieldSettings();
        public MetadataFieldSettings Genre
        {
            get => genre;
            set
            {
                genre = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings releaseDate = new MetadataFieldSettings();
        public MetadataFieldSettings ReleaseDate
        {
            get => releaseDate;
            set
            {
                releaseDate = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings developer = new MetadataFieldSettings();
        public MetadataFieldSettings Developer
        {
            get => developer;
            set
            {
                developer = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings publisher = new MetadataFieldSettings();
        public MetadataFieldSettings Publisher
        {
            get => publisher;
            set
            {
                publisher = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings tag = new MetadataFieldSettings();
        public MetadataFieldSettings Tag
        {
            get => tag;
            set
            {
                tag = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings feature = new MetadataFieldSettings();
        public MetadataFieldSettings Feature
        {
            get => feature;
            set
            {
                feature = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings description = new MetadataFieldSettings();
        public MetadataFieldSettings Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings coverImage = new MetadataFieldSettings();
        public MetadataFieldSettings CoverImage
        {
            get => coverImage;
            set
            {
                coverImage = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings backgroundImage = new MetadataFieldSettings();
        public MetadataFieldSettings BackgroundImage
        {
            get => backgroundImage;
            set
            {
                backgroundImage = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings icon = new MetadataFieldSettings();
        public MetadataFieldSettings Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings links = new MetadataFieldSettings();
        public MetadataFieldSettings Links
        {
            get => links;
            set
            {
                links = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings criticScore = new MetadataFieldSettings();
        public MetadataFieldSettings CriticScore
        {
            get => criticScore;
            set
            {
                criticScore = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings communityScore = new MetadataFieldSettings();
        public MetadataFieldSettings CommunityScore
        {
            get => communityScore;
            set
            {
                communityScore = value;
                OnPropertyChanged();
            }
        }

        public MetadataDownloaderSettings()
        {
        }

        public static MetadataDownloaderSettings GetDefaultSettings()
        {
            var igdbPluginId = BuiltinExtensions.GetIdFromExtension(BuiltinExtension.IgdbMetadata);
            var settings = new MetadataDownloaderSettings();
            settings.ConfigureFields(new List<Guid> { Guid.Empty, igdbPluginId }, true);
            settings.CoverImage.Sources = new List<Guid> { igdbPluginId, Guid.Empty };
            settings.Name.Import = false;
            return settings;
        }

        public void ConfigureFields(List<Guid> sources, bool import)
        {
            Name = new MetadataFieldSettings(import, sources);
            Genre = new MetadataFieldSettings(import, sources);
            Description = new MetadataFieldSettings(import, sources);
            Developer = new MetadataFieldSettings(import, sources);
            Publisher = new MetadataFieldSettings(import, sources);
            Tag = new MetadataFieldSettings(import, sources);
            Links = new MetadataFieldSettings(import, sources);
            CoverImage = new MetadataFieldSettings(import, sources);
            BackgroundImage = new MetadataFieldSettings(import, sources);
            Icon = new MetadataFieldSettings(import, sources);
            ReleaseDate = new MetadataFieldSettings(import, sources);
            CommunityScore = new MetadataFieldSettings(import, sources);
            CriticScore = new MetadataFieldSettings(import, sources);
            Feature = new MetadataFieldSettings(import, sources);
        }
    }
}
