using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Models.GitHub
{
    public class GitHubWebhook
    {
        [JsonProperty("ref")]
        public string referer { get; set; }
    }
}
