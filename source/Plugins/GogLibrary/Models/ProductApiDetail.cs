using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GogLibrary.Models
{
    public class ProductApiDetail
    {
        public class Compatibility
        {
            public bool windows;
            public bool osx;
            public bool linux;
        }

        public class Links
        {
            public string purchase_link;
            public string product_card;
            public string support;
            public string forum;
        }

        public class Images
        {
            public string background;
            public string logo;
            public string logo2x;
            public string icon;
            public string sidebarIcon;
            public string sidebarIcon2x;
        }

        public class Description
        {
            public string lead;
            public string full;
            public string whats_cool_about_it;
        }

        public int id;
        public string title;
        public string slug;
        public Compatibility content_system_compatibility;
        public Dictionary<string, string> languages;
        public Links links;
        public bool is_secret;
        public string game_type;
        public bool is_pre_order;
        public Images images;
        public Description description;
        public DateTime? release_date;
    }
}
