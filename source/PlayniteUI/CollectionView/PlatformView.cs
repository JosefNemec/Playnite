using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUI
{
    public class PlatformView : IComparable
    {
        public Platform Platform
        {
            get; set;
        }

        public Guid PlatformId
        {
            get; set;
        }

        public string Name
        {
            get => Platform?.Name ?? string.Empty;
        }

        public PlatformView(Guid platformId, Platform platform)
        {
            Platform = platform;
            PlatformId = platformId;
        }

        public int CompareTo(object obj)
        {
            var platform = (obj as PlatformView).Name;

            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(platform))
            {
                return 0;
            }
            if (string.IsNullOrEmpty(Name))
            {
                return 1;
            }
            if (string.IsNullOrEmpty(platform))
            {
                return -1;
            }
            if (Name.Equals(platform))
            {
                return 0;
            }

            return string.Compare(Name, platform, true);
        }

        public override bool Equals(object obj)
        {
            var platform = (obj as PlatformView).Name;

            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(platform))
            {
                return true;
            }
            if (string.IsNullOrEmpty(Name))
            {
                return false;
            }
            if (string.IsNullOrEmpty(platform))
            {
                return false;
            }
            if (Name.Equals(platform))
            {
                return true;
            }

            return string.Compare(Name, platform, true) == 0;
        }

        public override int GetHashCode()
        {
            if (Name == null)
            {
                return 0;
            }
            else
            {
                return Name.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? DefaultResourceProvider.FindString("LOCNoPlatform") : Name;
        }
    }
}
