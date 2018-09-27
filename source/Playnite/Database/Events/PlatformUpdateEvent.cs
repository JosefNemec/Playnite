using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database.Events
{
    public class PlatformUpdateEvent
    {
        public Platform OldData
        {
            get; set;
        }

        public Platform NewData
        {
            get; set;
        }

        public PlatformUpdateEvent(Platform oldData, Platform newData)
        {
            OldData = oldData;
            NewData = newData;
        }
    }

    public delegate void PlatformUpdatedEventHandler(object sender, PlatformUpdatedEventArgs args);

    public class PlatformUpdatedEventArgs : EventArgs
    {
        public List<PlatformUpdateEvent> UpdatedPlatforms
        {
            get; set;
        }

        public PlatformUpdatedEventArgs(Platform oldData, Platform newData)
        {
            UpdatedPlatforms = new List<PlatformUpdateEvent>() { new PlatformUpdateEvent(oldData, newData) };
        }

        public PlatformUpdatedEventArgs(List<PlatformUpdateEvent> updatedPlatforms)
        {
            UpdatedPlatforms = updatedPlatforms;
        }
    }
}
