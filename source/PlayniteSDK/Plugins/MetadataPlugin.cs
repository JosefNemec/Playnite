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
        ///
        Name,
        ///
        Genres,
        ///
        ReleaseDate,
        ///
        Developers,
        ///
        Publishers,
        ///
        Tags,
        ///
        Description,
        ///
        Links,
        ///
        CriticScore,
        ///
        CommunityScore,
        ///
        Icon,
        ///
        CoverImage,
        ///
        BackgroundImage,
        ///
        Features
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
        ///
        /// </summary>
        /// <returns></returns>
        public virtual string GetName()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetGenres()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual DateTime? GetReleaseDate()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetDevelopers()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetPublishers()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetTags()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual string GetDescription()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual int? GetCriticScore()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual int? GetCommunityScore()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual MetadataFile GetCoverImage()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual MetadataFile GetIcon()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual MetadataFile GetBackgroundImage()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual List<Link> GetLinks()
        {
            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetFeatures()
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
