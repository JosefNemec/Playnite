using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordLibrary.Models
{
    public class Application
    {
        public string cover_image; //number only
        public string description; //Markdown
        public List<Developer> developers;
        public List<Executable> executables;
        public string icon;
        public long id;
        public string name;
        public List<Publisher> publishers;
        public string slug;
        public long primary_sku_id;
        //public string summary;
        public List<string> aliases;
        public List<ThirdPartySku> third_party_skus;
        //public string splash;
        //public string youtube_trailer_video_id;

        public class Developer
        {
            public long id;
            public string name;
        }

        public class Executable
        {
            public string arguments;
            public string name;
            public string os;
        }

        public class Publisher
        {
            public long id;
            public string name;
        }

        public class ThirdPartySku
        {
            public string distributor;
            public string id;
            public string sku;
        }
    }
}
