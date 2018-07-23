using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Metadata
{
    public class MetadataFile
    {
        public string FileId
        {
            get; set;
        }

        public string FileName
        {
            get; set;
        }

        public byte[] Content
        {
            get; set;
        }

        public string OriginalUrl
        {
            get; set;
        }

        public MetadataFile()
        {
        }

        public MetadataFile(string url, string fileId)
        {
            FileId = fileId;
            FileName = Path.GetFileName(url);
        }

        public MetadataFile(string fileId, string name, byte[] data) : this(fileId, name, data, null)
        {            
        }

        public MetadataFile(string fileId, string name, byte[] data, string originalUrl)
        {
            FileId = fileId;
            FileName = name;
            Content = data;
            OriginalUrl = originalUrl;
        }
    }
}
