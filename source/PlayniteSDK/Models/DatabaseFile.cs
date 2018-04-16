using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents file stored in game database.
    /// </summary>
    public class DatabaseFile
    {
        /// <summary>
        /// Gets or sets file id.
        /// </summary>
        public string Id
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets file name.
        /// </summary>
        public string Filename
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets file mime type.
        /// </summary>
        public string MimeType
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets file size.
        /// </summary>
        public long Length
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets time when file was added into database.
        /// </summary>
        public DateTime UploadDate
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets file metadata.
        /// </summary>
        public Dictionary<string, object> Metadata
        {
            get;
            set;
        }

        /// <summary>
        /// Creates new instance of DatabaseFile.
        /// </summary>
        public DatabaseFile()
        {
        }

        /// <summary>
        /// Creates new instance of DatabaseFile based on file from LiteDB database.
        /// </summary>
        /// <param name="liteDbFile">LiteDB file.</param>
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
