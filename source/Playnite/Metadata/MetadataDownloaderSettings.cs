using System;
using System.Collections.Generic;
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
        Store,
        IGDB,
        IGDBOverStore,
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
                OnPropertyChanged("Import");
            }
        }

        private MetadataSource source = MetadataSource.StoreOverIGDB;
        public MetadataSource Source
        {
            get => source;
            set
            {
                source = value;
                OnPropertyChanged("Source");
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
        public MetadataGamesSource GamesSource
        {
            get
            {
                return gamesSource;
            }

            set
            {
                gamesSource = value;
                OnPropertyChanged("GamesSource");
            }
        }

        private bool skipExistingValues = true;
        public bool SkipExistingValues
        {
            get
            {
                return skipExistingValues;
            }

            set
            {
                skipExistingValues = value;
                OnPropertyChanged("SkipExistingValues");
            }
        }

        private MetadataFieldSettings name = new MetadataFieldSettings(false, MetadataSource.Store);
        public MetadataFieldSettings Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        private MetadataFieldSettings genre = new MetadataFieldSettings();
        public MetadataFieldSettings Genre
        {
            get => genre;
            set
            {
                genre = value;
                OnPropertyChanged("Genre");
            }
        }

        private MetadataFieldSettings releaseDate = new MetadataFieldSettings();
        public MetadataFieldSettings ReleaseDate
        {
            get => releaseDate;
            set
            {
                releaseDate = value;
                OnPropertyChanged("ReleaseDate");
            }
        }

        private MetadataFieldSettings developer = new MetadataFieldSettings();
        public MetadataFieldSettings Developer
        {
            get => developer;
            set
            {
                developer = value;
                OnPropertyChanged("Developer");
            }
        }

        private MetadataFieldSettings publisher = new MetadataFieldSettings();
        public MetadataFieldSettings Publisher
        {
            get => publisher;
            set
            {
                publisher = value;
                OnPropertyChanged("Publisher");
            }
        }

        private MetadataFieldSettings tag = new MetadataFieldSettings();
        public MetadataFieldSettings Tag
        {
            get => tag;
            set
            {
                tag = value;
                OnPropertyChanged("Tag");
            }
        }

        private MetadataFieldSettings description = new MetadataFieldSettings();
        public MetadataFieldSettings Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }

        private MetadataFieldSettings coverImage = new MetadataFieldSettings() { Source = MetadataSource.IGDBOverStore };
        public MetadataFieldSettings CoverImage
        {
            get => coverImage;
            set
            {
                coverImage = value;
                OnPropertyChanged("CoverImage");
            }
        }

        private MetadataFieldSettings backgroundImage = new MetadataFieldSettings() { Source = MetadataSource.Store };
        public MetadataFieldSettings BackgroundImage
        {
            get => backgroundImage;
            set
            {
                backgroundImage = value;
                OnPropertyChanged("BackgroundImage");
            }
        }

        private MetadataFieldSettings icon = new MetadataFieldSettings() { Source = MetadataSource.Store };
        public MetadataFieldSettings Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged("Icon");
            }
        }

        private MetadataFieldSettings links = new MetadataFieldSettings();
        public MetadataFieldSettings Links
        {
            get => links;
            set
            {
                links = value;
                OnPropertyChanged("Links");
            }
        }

        private MetadataFieldSettings criticScore = new MetadataFieldSettings();
        public MetadataFieldSettings CriticScore
        {
            get => criticScore;
            set
            {
                criticScore = value;
                OnPropertyChanged("CriticScore");
            }
        }

        private MetadataFieldSettings communityScore = new MetadataFieldSettings();
        public MetadataFieldSettings CommunityScore
        {
            get => communityScore;
            set
            {
                communityScore = value;
                OnPropertyChanged("CommunityScore");
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
