using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UplayLibrary.Models
{
    public class ProductInformation
    {
        public class Executable
        {
            public class Path
            {
                public string relative;
            }

            public class WorkingDirectory
            {
                public string register;
                public string append;
            }

            public Path path;
            public WorkingDirectory working_directory;
            public string internal_name;
            public string description;
            public string shortcut_name;
        }

        public class StartGameItem
        {
            public bool after_game_report_enabled;
            public bool overlay_supported;
            public bool overlay_product_activation_enabled;
            public bool overlay_required;
            public bool overlay_shop_enabled;
            public bool legacy_ticket_enabled;
            public List<Executable> executables;

        }

        public class StartGame
        {
            public StartGameItem online;
            public StartGameItem offline;
        }

        public class DigitalDistribution
        {
            public int version;
        }

        public class Localizations
        {
            public Dictionary<string, string> @default;
        }

        public class Club
        {
            public bool enabled;
        }

        public class Addon
        {
            public uint id;
            public bool is_visible;
            public string name;
            public string description;
            public string thumb_image;
        }

        public class Uplay
        {
            public string game_code;
            public string achievements;
            public string achievements_sync_id;
        }

        public class ThirdPartyPlatform
        {
            public string name;
        }

        public class Product
        {
            public string name;
            public string background_image;
            public string thumb_image;
            public string logo_image;
            public string dialog_image;
            public string icon_image;
            public ThirdPartyPlatform third_party_platform;
            public string sort_string;
            public bool cloud_saves;
            public string forum_url;
            public string homepage_url;
            public string facebook_url;
            public string help_url;
            public bool after_game_report_ad;
            public bool force_safe_mode;
            public bool uplay_pipe_required;
            public bool show_properties;
            public bool game_streaming_enabled;
            public Uplay uplay;
            public List<Addon> addons;
            public Club club;
            public DigitalDistribution digital_distribution;
            public bool is_ulc;
            public bool is_visible;
            public StartGame start_game;
        }

        public string version;
        public Product root;
        public Localizations localizations;
        public uint uplay_id;
        public uint install_id;
    }
}
