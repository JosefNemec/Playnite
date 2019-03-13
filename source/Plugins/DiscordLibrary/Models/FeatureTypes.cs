using System.ComponentModel;

namespace DiscordLibrary.Models
{
    // Enums extracted from https://discordapp.com/assets/012d4cdfa848e4ebc4a2.js

    // TODO: allow JSON desearlization to somehow gracefully fail if it isn't found in these enums, they won't necessarily be in sync 

    public enum FeatureType
    {
        [Description("Single Player")]
        SINGLE_PLAYER = 1,
        [Description("Online Multiplayer")]
        ONLINE_MULTIPLAYER = 2,
        [Description("Local Multiplayer")]
        LOCAL_MULTIPLAYER = 3,
        [Description("PvP")]
        PVP = 4,
        [Description("Local Coop")]
        LOCAL_COOP = 5,
        [Description("Cross Platform")]
        CROSS_PLATFORM = 6,
        [Description("Rich Presence")]
        RICH_PRESENCE = 7,
        [Description("Discord Game Invites")]
        DISCORD_GAME_INVITES = 8,
        [Description("Spectator Mode")]
        SPECTATOR_MODE = 9,
        [Description("Controller Support")]
        CONTROLLER_SUPPORT = 10,
        [Description("Cloud Saves")]
        CLOUD_SAVES = 11,
        [Description("Online Coop")]
        ONLINE_COOP = 12,
        [Description("Secure Networking")]
        SECURE_NETWORKING = 13
    }
}
