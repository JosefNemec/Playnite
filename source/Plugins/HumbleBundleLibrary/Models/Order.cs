using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleLibrary.Models
{
    class Order
    {
        [JsonProperty(PropertyName = "gamekey")]
        public string GameKey { get; set; }
        [JsonProperty(PropertyName = "product")]
        public Product Product { get; set; }

        [JsonProperty(PropertyName = "subproducts")]
        public List<SubProduct> SubProducts { get; set; }

        [JsonProperty(PropertyName = "created")]
        public DateTime Created { get; set; }

    }
}
