using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleLibrary.Models
{
    class File
    {
        [JsonProperty(PropertyName = "human_size")]
        public string HumanSize { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "file_size")]
        public long FileSize { get; set; }

        [JsonProperty(PropertyName = "url")]
        public FileUrlInfo Url {get; set; }

        [JsonProperty(PropertyName = "sha1")]
        public string Sha1 { get; set; }

        [JsonProperty(PropertyName = "md5")]
        public string Md5 { get; set; }
    }
}
