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
    }
}
