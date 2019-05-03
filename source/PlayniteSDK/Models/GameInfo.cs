using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class GameInfo : DatabaseObject
    {
        public string GameId { get; set; }
        public string Description { get; set; }
        public string InstallDirectory { get; set; }
        public string GameImagePath { get; set; }
        public string SortingName { get; set; }
        public List<GameAction> OtherActions { get; set; }
        public GameAction PlayAction { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public List<Link> Links { get; set; }
        public bool IsInstalled { get; set; }
        public long Playtime { get; set; }
        public long PlayCount { get; set; }
        public DateTime? LastActivity { get; set; }
        public CompletionStatus CompletionStatus { get; set; }
        public int? UserScore { get; set; }
        public int? CriticScore { get; set; }
        public int? CommunityScore { get; set; }
        public string Icon { get; set; }
        public string CoverImage { get; set; }
        public string BackgroundImage { get; set; }
        public bool Hidden { get; set; }
        public string Version { get; set; }

        public string Series { get; set; }
        public string AgeRating { get; set; }
        public string Region { get; set; }
        public string Source { get; set; }

        public string Platform { get; set; }
        public List<string> Developers { get; set; }
        public List<string> Publishers { get; set; }
        public List<string> Genres { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Tags { get; set; }

        public GameInfo()
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
