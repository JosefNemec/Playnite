using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database.Events
{
    public delegate void PlatformsCollectionChangedEventHandler(object sender, PlatformsCollectionChangedEventArgs args);

    public class PlatformsCollectionChangedEventArgs : EventArgs
    {
        public List<Platform> AddedPlatforms
        {
            get; set;
        }

        public List<Platform> RemovedPlatforms
        {
            get; set;
        }

        public PlatformsCollectionChangedEventArgs(List<Platform> addedPlatforms, List<Platform> removedPlatforms)
        {
            AddedPlatforms = addedPlatforms;
            RemovedPlatforms = removedPlatforms;
        }
    }
}
