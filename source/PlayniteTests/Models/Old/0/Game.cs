using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using Playnite.Providers.GOG;
using Playnite.Providers.Origin;
using Playnite.Providers.Steam;
using Playnite.Providers;
using Playnite.SDK.Models;

namespace Playnite.Models.Old0
{
    public class Game
    {
        [BsonId]
        public int Id
        {
            get; set;
        }

        public string BackgroundImage
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public List<string> Developers
        {
            get; set;
        }

        public List<string> Genres
        {
            get; set;
        }

        public bool Hidden
        {
            get; set;
        }

        public bool Favorite
        {
            get; set;
        }

        public string Icon
        {
            get; set;
        }

        public string Image
        {
            get; set;
        }

        public string InstallDirectory
        {
            get; set;
        }

        public DateTime? LastActivity
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public string ProviderId
        {
            get; set;
        }

        public ObservableCollection<GameTask> OtherTasks
        {
            get; set;
        }

        public GameTask PlayTask
        {
            get; set;
        }

        public Provider Provider
        {
            get; set;
        }

        public List<string> Publishers
        {
            get; set;
        }

        public DateTime? ReleaseDate
        {
            get; set;
        }

        public List<string> Categories
        {
            get; set;
        }

        public bool IsProviderDataUpdated
        {
            get; set;
        }

        public string CommunityHubUrl
        {
            get; set;
        }

        public string StoreUrl
        {
            get; set;
        }

        public string WikiUrl
        {
            get; set;
        }

    }
}
