using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleLibrary.Models
{
    public class TroveGame
    {
        public class CarouselContent
        {
            public List<string> screenshot;
        }

        public class Publisher
        {
            [JsonProperty("publisher-name")]
            public string publisher_name;
        }

        public class Developer
        {
            [JsonProperty("developer-name")]
            public string developer_name;
        }

        public string machine_name;
        public string image;

        [JsonProperty("human-name")]
        public string human_name;

        [JsonProperty("description-text")]
        public string description_text;

        [JsonProperty("carousel-content")]
        public CarouselContent carousel_content;

        public List<Developer> developers;

        public List<Publisher> publishers;
    }
}
