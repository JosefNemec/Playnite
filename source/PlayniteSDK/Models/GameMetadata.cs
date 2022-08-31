using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents metadata file.
    /// </summary>
    public class MetadataFile
    {
        /// <summary>
        /// Indicates whether metadata holds some content.
        /// </summary>
        public bool HasContent
        {
            get => !string.IsNullOrEmpty(FileName) && Content != null;
        }

        /// <summary>
        /// Indicates whether there's some source information for the file (content or URL).
        /// </summary>
        public bool HasImageData
        {
            get => HasContent || !string.IsNullOrEmpty(Path);
        }

        /// <summary>
        /// Gets or sets file name.
        /// </summary>
        public string FileName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets file content.
        /// </summary>
        public byte[] Content
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets original source url.
        /// </summary>
        public string Path
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataFile"/>.
        /// </summary>
        public MetadataFile()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataFile"/>.
        /// </summary>
        /// <param name="path">Source path (URL, URI, system path).</param>
        public MetadataFile(string path)
        {
            FileName = System.IO.Path.GetFileName(path);
            Path = path;
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataFile"/>.
        /// </summary>
        /// <param name="name">File name.</param>
        /// <param name="data">File content.</param>
        public MetadataFile(string name, byte[] data) : this(name, data, null)
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataFile"/>.
        /// </summary>
        /// <param name="name">File name.</param>
        /// <param name="data">File content.</param>
        /// <param name="originalUrl">Source url.</param>
        public MetadataFile(string name, byte[] data, string originalUrl)
        {
            FileName = name;
            Content = data;
            Path = originalUrl;
        }
    }

    /// <summary>
    /// Represents metadata property referencing database object by ID.
    /// </summary>
    public class MetadataIdProperty : MetadataProperty
    {
        /// <summary>
        /// Gets ID of referenced object.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Creates new instance of <see cref="MetadataIdProperty"/>.
        /// </summary>
        /// <param name="dbId"></param>
        public MetadataIdProperty(Guid dbId)
        {
            if (dbId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(dbId));
            }

            Id = dbId;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Id.ToString();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as MetadataIdProperty);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc/>
        public bool Equals(MetadataIdProperty other)
        {
            if (other == null)
            {
                return false;
            }

            return Id == other.Id;
        }
    }

    /// <summary>
    /// Represents metadata property referencing data by name.
    /// </summary>
    public class MetadataNameProperty : MetadataProperty, IEquatable<MetadataNameProperty>
    {
        /// <summary>
        /// Property name value.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Creates new instance of <see cref="MetadataNameProperty"/>.
        /// </summary>
        /// <param name="name"></param>
        public MetadataNameProperty(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as MetadataNameProperty);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <inheritdoc/>
        public bool Equals(MetadataNameProperty other)
        {
            if (other == null)
            {
                return false;
            }

            return Name == other.Name;
        }
    }

    /// <summary>
    /// Represents metadata property referencing specification object by id.
    /// </summary>
    public class MetadataSpecProperty : MetadataProperty, IEquatable<MetadataSpecProperty>
    {
        /// <summary>
        /// Gets specification id.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Creates new instance of <see cref="MetadataSpecProperty"/>.
        /// </summary>
        /// <param name="specId"></param>
        public MetadataSpecProperty(string specId)
        {
            if (string.IsNullOrWhiteSpace(specId))
            {
                throw new ArgumentNullException(nameof(specId));
            }

            Id = specId;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Id;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as MetadataSpecProperty);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc/>
        public bool Equals(MetadataSpecProperty other)
        {
            if (other == null)
            {
                return false;
            }

            return Id == other.Id;
        }
    }

    /// <summary>
    /// Represents base metadata property.
    /// </summary>
    public abstract class MetadataProperty
    {
    }

    /// <summary>
    /// Represents importable game data.
    /// </summary>
    public class GameMetadata
    {
        /// <summary>
        /// Gets or sets Name.
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
        /// Gets or sets install size in bytes of the game.
        /// </summary>
        public ulong? InstallSize { get; set; }

        /// <summary>
        /// Gets or sets Sorting Name.
        /// </summary>
        public string SortingName { get; set; }

        /// <summary>
        /// Gets or sets Other Actions.
        /// </summary>
        public List<GameAction> GameActions { get; set; }

        /// <summary>
        /// Gets or sets ReleaseDate.
        /// </summary>
        public ReleaseDate? ReleaseDate { get; set; }

        /// <summary>
        /// Gets or sets Links.
        /// </summary>
        public List<Link> Links { get; set; }

        /// <summary>
        /// Gets or sets Roms.
        /// </summary>
        public List<GameRom> Roms { get; set; }

        /// <summary>
        /// Gets or sets whether the game is installed.
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// Gets or sets Playtime.
        /// </summary>
        public ulong Playtime { get; set; }

        /// <summary>
        /// Gets or sets PlayCount.
        /// </summary>
        public ulong PlayCount { get; set; }

        /// <summary>
        /// Gets or sets LastActivity.
        /// </summary>
        public DateTime? LastActivity { get; set; }

        /// <summary>
        /// Gets or sets CompletionStatus.
        /// </summary>
        public MetadataProperty CompletionStatus { get; set; }

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
        public MetadataFile Icon { get; set; }

        /// <summary>
        /// Gets or sets CoverImage.
        /// </summary>
        public MetadataFile CoverImage { get; set; }

        /// <summary>
        /// Gets or sets BackgroundImage.
        /// </summary>
        public MetadataFile BackgroundImage { get; set; }

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
        /// Gets or sets Source.
        /// </summary>
        public MetadataProperty Source { get; set; }

        /// <summary>
        /// Gets or sets Series.
        /// </summary>
        public HashSet<MetadataProperty> Series { get; set; }

        /// <summary>
        /// Gets or sets AgeRating.
        /// </summary>
        public HashSet<MetadataProperty> AgeRatings { get; set; }

        /// <summary>
        /// Gets or sets Region.
        /// </summary>
        public HashSet<MetadataProperty> Regions { get; set; }

        /// <summary>
        /// Gets or sets Platform.
        /// </summary>
        public HashSet<MetadataProperty> Platforms { get; set; }

        /// <summary>
        /// Gets or sets Developers.
        /// </summary>
        public HashSet<MetadataProperty> Developers { get; set; }

        /// <summary>
        /// Gets or sets Publishers.
        /// </summary>
        public HashSet<MetadataProperty> Publishers { get; set; }

        /// <summary>
        /// Gets or sets Genres.
        /// </summary>
        public HashSet<MetadataProperty> Genres { get; set; }

        /// <summary>
        /// Gets or sets Categories.
        /// </summary>
        public HashSet<MetadataProperty> Categories { get; set; }

        /// <summary>
        /// Gets or sets Tags.
        /// </summary>
        public HashSet<MetadataProperty> Tags { get; set; }

        /// <summary>
        /// Gets or sets game Features.
        /// </summary>
        public HashSet<MetadataProperty> Features { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="GameMetadata"/>.
        /// </summary>
        public GameMetadata()
        {
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
