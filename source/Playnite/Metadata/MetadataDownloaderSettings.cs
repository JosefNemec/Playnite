using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

    public enum MetadataSource
    {
        [Description("LOCMetaSourceStore")]
        Store,
        [Description("LOCMetaSourceIGDB")]
        IGDB,
        [Description("LOCMetaSourceStoreOverIGDB")]
        IGDBOverStore,
        [Description("LOCMetaSourceStoreOverIGDB")]
        StoreOverIGDB
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

        private MetadataSource source = MetadataSource.StoreOverIGDB;
        public MetadataSource Source
        {
            get => source;
            set
            {
                source = value;
                OnPropertyChanged();
            }
        }

        public MetadataFieldSettings()
        {
        }

        public MetadataFieldSettings(bool import, MetadataSource source)
        {
            Import = import;
            Source = source;
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

        private MetadataFieldSettings name = new MetadataFieldSettings(false, MetadataSource.Store);
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

        private MetadataFieldSettings coverImage = new MetadataFieldSettings() { Source = MetadataSource.IGDBOverStore };
        public MetadataFieldSettings CoverImage
        {
            get => coverImage;
            set
            {
                coverImage = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings backgroundImage = new MetadataFieldSettings() { Source = MetadataSource.Store };
        public MetadataFieldSettings BackgroundImage
        {
            get => backgroundImage;
            set
            {
                backgroundImage = value;
                OnPropertyChanged();
            }
        }

        private MetadataFieldSettings icon = new MetadataFieldSettings() { Source = MetadataSource.Store };
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

        public void ConfigureFields(MetadataSource source, bool import)
        {
            Genre.Import = import;
            Genre.Source = source;
            Description.Import = import;
            Description.Source = source;
            Developer.Import = import;
            Developer.Source = source;
            Publisher.Import = import;
            Publisher.Source = source;
            Tag.Import = import;
            Tag.Source = source;
            Links.Import = import;
            Links.Source = source;
            CoverImage.Import = import;
            CoverImage.Source = source;
            BackgroundImage.Import = import;
            BackgroundImage.Source = source;
            Icon.Import = import;
            Icon.Source = source;
            ReleaseDate.Import = import;
            ReleaseDate.Source = source;
            CommunityScore.Import = import;
            CommunityScore.Source = source;
            CriticScore.Import = import;
            CriticScore.Source = source;
        }
    }
}
