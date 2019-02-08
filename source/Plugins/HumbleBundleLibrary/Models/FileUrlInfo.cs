using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleLibrary.Models
{
    class FileUrlInfo
    {
        [JsonProperty(PropertyName = "web")]
        public string Web { get; set; }

        [JsonProperty(PropertyName = "bittorrent")]
        public string BitTorrent{ get; set; }
    }
}
