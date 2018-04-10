using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class DatabaseFile
    {
        public string Id
        {
            get; set;
        }

        public string Filename
        {
            get; set;
        }

        public string MimeType
        {
            get; set;
        }

        public long Length
        {
            get; set;
        }

        public DateTime UploadDate
        {
            get; set;
        }

        public Dictionary<string, object> Metadata
        {
            get;
            set;
        }

        public DatabaseFile()
        {
        }

        public DatabaseFile(LiteFileInfo liteDbFile)
        {
            Id = liteDbFile.Id;
            Filename = liteDbFile.Filename;
            MimeType = liteDbFile.MimeType;
            Length = liteDbFile.Length;
            UploadDate = liteDbFile.UploadDate;
            if (liteDbFile.Metadata != null)
            {
                Metadata = new Dictionary<string, object>();
                foreach (var key in liteDbFile.Metadata.Keys)
                {
                    Metadata.Add(key, liteDbFile.Metadata[key]);
                }
            }
        }
    }
}
