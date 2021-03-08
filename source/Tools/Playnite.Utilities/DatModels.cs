using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Utilities
{
    public class DatGameRom
    {
        [DatProperty("crc")]
        public string Crc { get; set; }

        [DatProperty("md5")]
        public string Md5 { get; set; }

        [DatProperty("sha1")]
        public string Sha1 { get; set; }

        [DatProperty("serial")]
        public string Serial { get; set; }

        [DatProperty("name")]
        public string Name { get; set; }

        [DatProperty("size")]
        public long Size { get; set; }

        public override string ToString()
        {
            return Name ?? Crc ?? Md5;
        }
    }

    public class DatGame
    {
        [DatProperty("name")]
        public string Name { get; set; }

        [DatProperty("description")]
        public string Description { get; set; }

        [DatProperty("comment")]
        public string Comment { get; set; }

        [DatProperty("developer")]
        public string Developer { get; set; }

        [DatProperty("publisher")]
        public string Publisher { get; set; }

        [DatProperty("genre")]
        public string Genre { get; set; }

        [DatProperty("users")]
        public uint Users { get; set; }

        [DatProperty("origin")]
        public string Origin { get; set; }

        [DatProperty("region")]
        public string Region { get; set; }

        [DatProperty("franchise")]
        public string Franchise { get; set; }

        [DatProperty("releaseyear")]
        public string ReleaseYear { get; set; }

        [DatProperty("releasemonth")]
        public string ReleaseMonth { get; set; }

        [DatProperty("rom", true)]
        public List<DatGameRom> Roms { get; set; }

        [DatProperty("serial")]
        public string Serial { get; set; }

        public override string ToString()
        {
            return Name ?? base.ToString();
        }
    }

    public class DatPropertyAttribute : Attribute
    {
        public string Name { get; }
        public bool IsList { get; }

        public DatPropertyAttribute(string name, bool isList = false)
        {
            Name = name;
            IsList = isList;
        }
    }
}
