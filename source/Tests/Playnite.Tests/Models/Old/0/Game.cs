using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using LiteDB;
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

        public ObservableCollection<GameAction> OtherTasks
        {
            get; set;
        }

        public GameAction PlayTask
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
