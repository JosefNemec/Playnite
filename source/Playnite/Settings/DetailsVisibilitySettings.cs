using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class DetailsVisibilitySettings : ObservableObject
    {
        private bool library = true;
        public bool Library
        {
            get
            {
                return library;
            }

            set
            {
                library = value;
                OnPropertyChanged();
            }
        }

        private bool playTime = true;
        public bool PlayTime
        {
            get
            {
                return playTime;
            }

            set
            {
                playTime = value;
                OnPropertyChanged();
            }
        }

        private bool lastPlayed = true;
        public bool LastPlayed
        {
            get
            {
                return lastPlayed;
            }

            set
            {
                lastPlayed = value;
                OnPropertyChanged();
            }
        }

        private bool completionStatus = false;
        public bool CompletionStatus
        {
            get
            {
                return completionStatus;
            }

            set
            {
                completionStatus = value;
                OnPropertyChanged();
            }
        }

        private bool icon = false;
        public bool Icon
        {
            get
            {
                return icon;
            }

            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        private bool coverImage = true;
        public bool CoverImage
        {
            get
            {
                return coverImage;
            }

            set
            {
                coverImage = value;
                OnPropertyChanged();
            }
        }

        private bool backgroundImage = true;
        public bool BackgroundImage
        {
            get
            {
                return backgroundImage;
            }

            set
            {
                backgroundImage = value;
                OnPropertyChanged();
            }
        }

        private bool platform = true;
        public bool Platform
        {
            get
            {
                return platform;
            }

            set
            {
                platform = value;
                OnPropertyChanged();
            }
        }

        private bool genres = true;
        public bool Genres
        {
            get
            {
                return genres;
            }

            set
            {
                genres = value;
                OnPropertyChanged();
            }
        }

        private bool developers = true;
        public bool Developers
        {
            get
            {
                return developers;
            }

            set
            {
                developers = value;
                OnPropertyChanged();
            }
        }

        private bool publishers = true;
        public bool Publishers
        {
            get
            {
                return publishers;
            }

            set
            {
                publishers = value;
                OnPropertyChanged();
            }
        }

        private bool releaseDate = true;
        public bool ReleaseDate
        {
            get
            {
                return releaseDate;
            }

            set
            {
                releaseDate = value;
                OnPropertyChanged();
            }
        }

        private bool categories = true;
        public bool Categories
        {
            get
            {
                return categories;
            }

            set
            {
                categories = value;
                OnPropertyChanged();
            }
        }

        private bool tags = true;
        public bool Tags
        {
            get
            {
                return tags;
            }

            set
            {
                tags = value;
                OnPropertyChanged();
            }
        }

        private bool links = true;
        public bool Links
        {
            get
            {
                return links;
            }

            set
            {
                links = value;
                OnPropertyChanged();
            }
        }

        private bool description = true;
        public bool Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
                OnPropertyChanged();
            }
        }

        private bool ageRating = false;
        public bool AgeRating
        {
            get
            {
                return ageRating;
            }

            set
            {
                ageRating = value;
                OnPropertyChanged();
            }
        }

        private bool series = false;
        public bool Series
        {
            get
            {
                return series;
            }

            set
            {
                series = value;
                OnPropertyChanged();
            }
        }

        private bool source = false;
        public bool Source
        {
            get
            {
                return source;
            }

            set
            {
                source = value;
                OnPropertyChanged();
            }
        }

        private bool region = false;
        public bool Region
        {
            get
            {
                return region;
            }

            set
            {
                region = value;
                OnPropertyChanged();
            }
        }

        private bool version = false;
        public bool Version
        {
            get
            {
                return version;
            }

            set
            {
                version = value;
                OnPropertyChanged();
            }
        }

        private bool communityScore = false;
        public bool CommunityScore
        {
            get
            {
                return communityScore;
            }

            set
            {
                communityScore = value;
                OnPropertyChanged();
            }
        }

        private bool criticScore = false;
        public bool CriticScore
        {
            get
            {
                return criticScore;
            }

            set
            {
                criticScore = value;
                OnPropertyChanged();
            }
        }

        private bool userScore = false;
        public bool UserScore
        {
            get
            {
                return userScore;
            }

            set
            {
                userScore = value;
                OnPropertyChanged();
            }
        }

        private bool features = true;
        public bool Features
        {
            get
            {
                return features;
            }

            set
            {
                features = value;
                OnPropertyChanged();
            }
        }
    }
}
