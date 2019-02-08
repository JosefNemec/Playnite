using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleBundleLibrary.Models
{
    class GameKeyInfo
    {
        [JsonProperty(PropertyName = "gamekey")]
        public string GameKey { get; set; }
    }
}
