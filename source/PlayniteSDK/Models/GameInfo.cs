using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents importable game data.
    /// </summary>
    public class GameInfo
    {
        /// <summary>
        /// Gets or sets name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets custom game identifier.
        /// </summary>
        public string GameId { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets installation directory.
        /// </summary>
        public string InstallDirectory { get; set; }

        /// <summary>
        /// Gets or sets game image (rom, iso) path.
        /// </summary>
        public string GameImagePath { get; set; }

        /// <summary>
        /// Gets or sets Sorting Name.
        /// </summary>
        public string SortingName { get; set; }

        /// <summary>
        /// Gets or sets Other Actions.
        /// </summary>
        public List<GameAction> OtherActions { get; set; }

        /// <summary>
        /// Gets or sets PlayAction.
        /// </summary>
        public GameAction PlayAction { get; set; }

        /// <summary>
        /// Gets or sets ReleaseDate.
        /// </summary>
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// Gets or sets Links.
        /// </summary>
        public List<Link> Links { get; set; }

        /// <summary>
        /// Gets or sets whether the game is installed.
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// Gets or sets Playtime.
        /// </summary>
        public long Playtime { get; set; }

        /// <summary>
        /// Gets or sets PlayCount.
        /// </summary>
        public long PlayCount { get; set; }

        /// <summary>
        /// Gets or sets LastActivity.
        /// </summary>
        public DateTime? LastActivity { get; set; }

        /// <summary>
        /// Gets or sets CompletionStatus.
        /// </summary>
        public CompletionStatus CompletionStatus { get; set; }

        /// <summary>
        /// Gets or sets UserScore.
        /// </summary>
        public int? UserScore { get; set; }

        /// <summary>
        /// Gets or sets CriticScore.
        /// </summary>
        public int? CriticScore { get; set; }

        /// <summary>
        /// Gets or sets CommunityScore.
        /// </summary>
        public int? CommunityScore { get; set; }

        /// <summary>
        /// Gets or sets Icon.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets CoverImage.
        /// </summary>
        public string CoverImage { get; set; }

        /// <summary>
        /// Gets or sets BackgroundImage.
        /// </summary>
        public string BackgroundImage { get; set; }

        /// <summary>
        /// Gets or sets Hidden.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets Favorite.
        /// </summary>
        public bool Favorite { get; set; }

        /// <summary>
        /// Gets or sets Version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets Series.
        /// </summary>
        public string Series { get; set; }

        /// <summary>
        /// Gets or sets AgeRating.
        /// </summary>
        public string AgeRating { get; set; }

        /// <summary>
        /// Gets or sets Region.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Gets or sets Source.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets Platform.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Gets or sets Developers.
        /// </summary>
        public List<string> Developers { get; set; }

        /// <summary>
        /// Gets or sets Publishers.
        /// </summary>
        public List<string> Publishers { get; set; }

        /// <summary>
        /// Gets or sets Genres.
        /// </summary>
        public List<string> Genres { get; set; }

        /// <summary>
        /// Gets or sets Categories.
        /// </summary>
        public List<string> Categories { get; set; }

        /// <summary>
        /// Gets or sets Tags.
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Gets or sets game Features.
        /// </summary>
        public List<string> Features { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="GameInfo"/>.
        /// </summary>
        public GameInfo()
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
