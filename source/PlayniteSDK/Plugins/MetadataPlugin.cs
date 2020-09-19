using Playnite.SDK.Metadata;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Plugins
{
    /// <summary>
    /// Represents metadata game fields.
    /// </summary>
    public enum MetadataField
    {
        /// <summary>
        /// Name can be provided.
        /// </summary>
        Name,
        /// <summary>
        /// Genres can be provided.
        /// </summary>
        Genres,
        /// <summary>
        /// Release Data can be provided.
        /// </summary>
        ReleaseDate,
        /// <summary>
        /// List of Developers can be provided.
        /// </summary>
        Developers,
        /// <summary>
        /// List of Publishers can be provided.
        /// </summary>
        Publishers,
        /// <summary>
        /// List of Tags can be provided.
        /// </summary>
        Tags,
        /// <summary>
        /// Description can be provided.
        /// </summary>
        Description,
        /// <summary>
        /// List of associated Links can be provided.
        /// </summary>
        Links,
        /// <summary>
        /// Critics Score can be provided.
        /// </summary>
        CriticScore,
        /// <summary>
        /// Community Score can be provided.
        /// </summary>
        CommunityScore,
        /// <summary>
        /// Icon can be provided.
        /// </summary>
        Icon,
        /// <summary>
        /// Cover Image can be provided.
        /// </summary>
        CoverImage,
        /// <summary>
        /// Background Image can be provided.
        /// </summary>
        BackgroundImage,
        /// <summary>
        /// List of Features can be provided.
        /// </summary>
        Features,
        /// <summary>
        /// Age Rating can be provided.
        /// </summary>
        AgeRating,
        /// <summary>
        /// Name of the Series can be provided.
        /// </summary>
        Series,
        /// <summary>
        /// Region can be provided.
        /// </summary>
        Region,
        /// <summary>
        /// Platform can be provided.
        /// </summary>
        Platform
    }

    /// <summary>
    /// Represents options for game metadata download.
    /// </summary>
    public class MetadataRequestOptions
    {
        /// <summary>
        /// Gets or sets game data are being requested for.
        /// </summary>
        public Game GameData
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets value indicating whether the request is part of bulk metadata download.
        /// </summary>
        public bool IsBackgroundDownload
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataRequestOptions"/>.
        /// </summary>
        /// <param name="gameData"></param>
        /// <param name="backgroundDownload"></param>
        public MetadataRequestOptions(Game gameData, bool backgroundDownload)
        {
            GameData = gameData;
            IsBackgroundDownload = backgroundDownload;
        }
    }

    /// <summary>
    /// Represents metadata class providing specific fields when requested.
    /// </summary>
    public abstract class OnDemandMetadataProvider : IDisposable
    {
        /// <summary>
        ///  Gets currently available fields.
        /// </summary>
        public abstract List<MetadataField> AvailableFields { get; }

        /// <inheritdoc />
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Gets name.
        /// </summary>
        /// <returns></returns>
        public virtual string GetName()
        {
            return null;
        }

        /// <summary>
        /// Gets genres.
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetGenres()
        {
            return null;
        }

        /// <summary>
        /// Gets relese date.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime? GetReleaseDate()
        {
            return null;
        }

        /// <summary>
        /// Gets developers.
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetDevelopers()
        {
            return null;
        }

        /// <summary>
        /// Gets publishers.
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetPublishers()
        {
            return null;
        }

        /// <summary>
        /// Gets tags.
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetTags()
        {
            return null;
        }

        /// <summary>
        /// Gets descriptions.
        /// </summary>
        /// <returns></returns>
        public virtual string GetDescription()
        {
            return null;
        }

        /// <summary>
        /// Gets critic score.
        /// </summary>
        /// <returns></returns>
        public virtual int? GetCriticScore()
        {
            return null;
        }

        /// <summary>
        /// Gets community score.
        /// </summary>
        /// <returns></returns>
        public virtual int? GetCommunityScore()
        {
            return null;
        }

        /// <summary>
        /// Gets cover image.
        /// </summary>
        /// <returns></returns>
        public virtual MetadataFile GetCoverImage()
        {
            return null;
        }

        /// <summary>
        /// Gets icon image.
        /// </summary>
        /// <returns></returns>
        public virtual MetadataFile GetIcon()
        {
            return null;
        }

        /// <summary>
        /// Gets background image.
        /// </summary>
        /// <returns></returns>
        public virtual MetadataFile GetBackgroundImage()
        {
            return null;
        }

        /// <summary>
        /// Gets links.
        /// </summary>
        /// <returns></returns>
        public virtual List<Link> GetLinks()
        {
            return null;
        }

        /// <summary>
        /// Gets features.
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetFeatures()
        {
            return null;
        }

        /// <summary>
        /// Gets age rating.
        /// </summary>
        /// <returns></returns>
        public virtual string GetAgeRating()
        {
            return null;
        }

        /// <summary>
        /// Gets series.
        /// </summary>
        /// <returns></returns>
        public virtual string GetSeries()
        {
            return null;
        }

        /// <summary>
        /// Gets region.
        /// </summary>
        /// <returns></returns>
        public virtual string GetRegion()
        {
            return null;
        }

        /// <summary>
        /// Gets platform.
        /// </summary>
        /// <returns></returns>
        public virtual string GetPlatform()
        {
            return null;
        }
    }

    /// <summary>
    /// Represents plugin providing game metadata.
    /// </summary>
    public abstract class MetadataPlugin : Plugin
    {
        /// <summary>
        /// Gets metadata source name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets list of game fields this metadata provider can provide.
        /// </summary>
        public abstract List<MetadataField> SupportedFields { get; }

        /// <summary>
        /// Creates new instance of <see cref="MetadataPlugin"/>.
        /// </summary>
        /// <param name="playniteAPI"></param>
        public MetadataPlugin(IPlayniteAPI playniteAPI) : base(playniteAPI)
        {
        }

        /// <summary>
        /// Gets metadata provider.
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public abstract OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options);
    }
}
