using SqlNado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Playnite.Emulators
{
    public class DatGame
    {
        [SQLiteColumn(IsPrimaryKey = true, AutoIncrements = true)]
        public int Id { get; set; }

        [DatProperty("name")]
        public string Name { get; set; }

        [DatProperty("region")]
        public string Region { get; set; }

        [DatProperty("releaseyear")]
        public string ReleaseYear { get; set; }

        [SQLiteIndex(nameof(Serial))]
        [DatProperty("serial")]
        public string Serial { get; set; }

        [SQLiteIndex(nameof(RomCrc))]
        [DatProperty("rom.crc")]
        public string RomCrc { get; set; }

        [DatProperty("rom.name")]
        public string RomName { get; set; }

        [YamlIgnore]
        [SQLiteColumn(Ignore = true)]
        [DatProperty("rom.serial")]
        public string RomSerial { get; set; }

        [YamlIgnore]
        [SQLiteColumn(Ignore = true)]
        [DatProperty("origin")]
        public string Origin { get; set; }

        [YamlIgnore]
        [SQLiteColumn(Ignore = true)]
        [DatProperty("comment")]
        public string Comment { get; set; }

        //[DatProperty("rom.size")]
        //public long RomSize { get; set; }

        //[DatProperty("rom.md5")]
        //public string RomMd5 { get; set; }

        //[DatProperty("rom.sha1")]
        //public string RomSha1 { get; set; }

        [YamlIgnore]
        [SQLiteColumn(Ignore = true)]
        public string SanitizedName => Name.IsNullOrEmpty() ? string.Empty : Emulators.RomName.SanitizeName(Name);

        public override string ToString()
        {
            return $"{Serial} {RomCrc} {Name}";
        }

        public void CopyTo(DatGame target)
        {
            if (!Name.IsNullOrEmpty() && target.Name.IsNullOrEmpty())
            {
                target.Name = Name;
            }

            if (!Origin.IsNullOrEmpty() && Region.IsNullOrEmpty() && target.Region.IsNullOrEmpty())
            {
                target.Region = Origin;
            }

            if (!Region.IsNullOrEmpty() && target.Region.IsNullOrEmpty())
            {
                target.Region = Region;
            }

            if (!ReleaseYear.IsNullOrEmpty() && target.ReleaseYear.IsNullOrEmpty())
            {
                target.ReleaseYear = ReleaseYear;
            }

            if (!RomSerial.IsNullOrEmpty() && target.Serial.IsNullOrEmpty())
            {
                target.Serial = RomSerial;
            }

            if (!Serial.IsNullOrEmpty() && target.Serial.IsNullOrEmpty())
            {
                target.Serial = Serial;
            }
        }

        public void FixData()
        {
            if (Serial.IsNullOrEmpty() && !RomSerial.IsNullOrEmpty())
            {
                Serial = RomSerial;
            }

            if (Region.IsNullOrEmpty() && !Origin.IsNullOrEmpty())
            {
                Region = Origin;
            }

            if (Name.IsNullOrEmpty() && !Comment.IsNullOrEmpty())
            {
                Name = Comment;
            }
        }
    }

    public class DatPropertyAttribute : Attribute
    {
        public string Name { get; }

        public DatPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}
