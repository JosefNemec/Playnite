using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGDBMetadata.Models
{
    /// <summary>
    /// Represents metadata search result.
    /// </summary>
    public class SearchResult : GenericItemOption
    {
        /// <summary>
        /// Gets or sets result id.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets release date.
        /// </summary>
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// Gets or sets alternate game names.
        /// </summary>
        public List<string> AlternativeNames { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="SearchResult"/>.
        /// </summary>
        public SearchResult()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="SearchResult"/>.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        public SearchResult(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Creates new instance of <see cref="SearchResult"/>.
        /// </summary>
        /// <param name="id">Result id.</param>
        /// <param name="name">Game name.</param>
        /// <param name="releaseDate">Release date.</param>
        /// <param name="alternativeNames">Alternate game names.</param>
        /// <param name="description"></param>
        public SearchResult(string id, string name, DateTime? releaseDate, List<string> alternativeNames, string description)
        {
            Id = id;
            Name = name;
            ReleaseDate = releaseDate;
            AlternativeNames = alternativeNames;
            Description = description;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }
    }
}
