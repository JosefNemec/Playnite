using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleLibrary.Models
{
    class SubProduct
    {
        [JsonProperty(PropertyName = "human_name")]
        public string HumanName { get; set; }

        [JsonProperty(PropertyName = "machine_name")]
        public string MachineName { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "downloads")]
        public List<Download> Downloads { get; set; }

        [JsonProperty(PropertyName = "payee")]
        public PayeeInfo Payee { get; set; }
    }
}
