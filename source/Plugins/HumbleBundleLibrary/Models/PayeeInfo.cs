using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleLibrary.Models
{
    class PayeeInfo
    {
        [JsonProperty(PropertyName = "human_name")]
        public string HumanName { get; set; }

        [JsonProperty(PropertyName = "machine_name")]
        public string MachineName { get; set; }
    }
}
