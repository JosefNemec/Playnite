using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Metadata
{
    /// <summary>
    /// Represents metadata search result.
    /// </summary>
    public class MetadataSearchResult
    {
        /// <summary>
        /// Gets or sets result id.
        /// </summary>
        public string Id
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets game name.
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets release date.
        /// </summary>
        public DateTime? ReleaseDate
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets alternate game names.
        /// </summary>
        public List<string> AlternativeNames
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataSearchResult"/>.
        /// </summary>
        public MetadataSearchResult()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="MetadataSearchResult"/>.
        /// </summary>
        /// <param name="id">Result id.</param>
        /// <param name="name">Game name.</param>
        /// <param name="releaseDate">Release date.</param>
        /// <param name="alternativeNames">Alternate game names.</param>
        public MetadataSearchResult(string id, string name, DateTime? releaseDate, List<string> alternativeNames)
        {
            Id = id;
            Name = name;
            ReleaseDate = releaseDate;
            AlternativeNames = alternativeNames;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
