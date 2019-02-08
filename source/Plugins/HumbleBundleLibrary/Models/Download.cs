using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleLibrary.Models
{
    class Download
    {
        [JsonProperty(PropertyName = "android_app_only")]
        public bool AndroidAppOnly { get; set; }

        [JsonProperty(PropertyName = "machine_name")]
        public string MachineName { get; set; }

        [JsonProperty(PropertyName = "platform")]
        public string Platform { get; set; }

        [JsonProperty(PropertyName = "download_struct")]
        public List<File> Files { get; set; }
    }
}
