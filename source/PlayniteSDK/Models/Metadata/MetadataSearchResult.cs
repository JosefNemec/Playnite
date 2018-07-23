using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Metadata
{
    public class MetadataSearchResult
    {
        public string Id
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public DateTime? ReleaseDate
        {
            get; set;
        }

        public List<string> AlternativeNames
        {
            get; set;
        }

        public MetadataSearchResult()
        {
        }

        public MetadataSearchResult(string id, string name, DateTime? releaseDate, List<string> alternativeNames)
        {
            Id = id;
            Name = name;
            ReleaseDate = releaseDate;
            AlternativeNames = alternativeNames;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
