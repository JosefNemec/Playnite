using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SteamLibrary.Models
{
    public class GetOwnedGamesResult
    {
        public class Game
        {
            public int appid;
            public string name;
            public int playtime_forever;
            public string img_icon_url;
            public string img_logo_url;
            public bool has_community_visible_stats;
        }

        public class Response
        {
            public int game_count;
            public List<Game> games;
        }

        public Response response;
    }
}
