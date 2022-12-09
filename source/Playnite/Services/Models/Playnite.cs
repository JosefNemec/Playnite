using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteServices.Models
{
    public class User
    {
        public string? Id { get; set; }
        public string? WinVersion { get; set; }
        public string? PlayniteVersion { get; set; }
        public DateTime LastLaunch { get; set; }
        public bool Is64Bit { get; set; }
    }

    public class PlayniteGame
    {
        public string? Name { get; set; }
        public ReleaseDate? ReleaseDate { get; set; }
        public Guid PluginId { get; set; }
        public string? GameId { get; set; }

        public PlayniteGame()
        {
        }

        public PlayniteGame(string name)
        {
            Name = name;
        }
    }

    public class PlayniteGame_OldV2
    {
        public string? Name { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public Guid PluginId { get; set; }
        public string? GameId { get; set; }
    }
}
