using System.ComponentModel;

namespace DiscordLibrary.Models
{
    // Enums extracted from https://discordapp.com/assets/012d4cdfa848e4ebc4a2.js

    // TODO: allow JSON desearlization to somehow gracefully fail if it isn't found in these enums, they won't necessarily be in sync 

    public enum Genre
    {
        [Description("Action")]
        ACTION = 1,
        [Description("Action RPG")]
        ACTION_RPG = 2,
        [Description("Brawler")]
        BRAWLER = 3,
        [Description("Hack and Slash")]
        HACK_AND_SLASH = 4,
        [Description("Platformer")]
        PLATFORMER = 5,
        [Description("Stealth")]
        STEALTH = 6,
        [Description("Survival")]
        SURVIVAL = 7,
        [Description("Adventure")]
        ADVENTURE = 8,
        [Description("Action Adventure")]
        ACTION_ADVENTURE = 9,
        [Description("Metroidvania")]
        METROIDVANIA = 10,
        [Description("Open World")]
        OPEN_WORLD = 11,
        [Description("Psychological Horror")]
        PSYCHOLOGICAL_HORROR = 12,
        [Description("Sandbox")]
        SANDBOX = 13,
        [Description("Survival Horror")]
        SURVIVAL_HORROR = 14,
        [Description("Visual Novel")]
        VISUAL_NOVEL = 15,
        [Description("Driving Racing")]
        DRIVING_RACING = 16,
        [Description("Vehicular Combat")]
        VEHICULAR_COMBAT = 17,
        [Description("Massively Multiplayer")]
        MASSIVELY_MULTIPLAYER = 18,
        [Description("MMORPG")]
        MMORPG = 19,
        [Description("Role Playing")]
        ROLE_PLAYING = 20,
        [Description("Dungeon Crawler")]
        DUNGEON_CRAWLER = 21,
        [Description("Roguelike")]
        ROGUELIKE = 22,
        [Description("Shooter")]
        SHOOTER = 23,
        [Description("Light Gun")]
        LIGHT_GUN = 24,
        [Description("Shoot Em up")]
        SHOOT_EM_UP = 25,
        [Description("FPS")]
        FPS = 26,
        [Description("Dual Joystick Shooter")]
        DUAL_JOYSTICK_SHOOTER = 27,
        [Description("Simulation")]
        SIMULATION = 28,
        [Description("Flight Simulator")]
        FLIGHT_SIMULATOR = 29,
        [Description("Train Simulator")]
        TRAIN_SIMULATOR = 30,
        [Description("Life Simulator")]
        LIFE_SIMULATOR = 31,
        [Description("Fishing")]
        FISHING = 32,
        [Description("Sports")]
        SPORTS = 33,
        [Description("Baseball")]
        BASEBALL = 34,
        [Description("Basketball")]
        BASKETBALL = 35,
        [Description("Billiards")]
        BILLIARDS = 36,
        [Description("Bowling")]
        BOWLING = 37,
        [Description("Boxing")]
        BOXING = 38,
        [Description("Football")]
        FOOTBALL = 39,
        [Description("Golf")]
        GOLF = 40,
        [Description("Hockey")]
        HOCKEY = 41,
        [Description("Skateboarding Skating")]
        SKATEBOARDING_SKATING = 42,
        [Description("Snowboarding Skiing")]
        SNOWBOARDING_SKIING = 43,
        [Description("Soccer")]
        SOCCER = 44,
        [Description("Track Field")]
        TRACK_FIELD = 45,
        [Description("Surfing Wakeboarding")]
        SURFING_WAKEBOARDING = 46,
        [Description("Wrestling")]
        WRESTLING = 47,
        [Description("Strategy")]
        STRATEGY = 48,
        [Description("4X")]
        FOUR_X = 49,
        [Description("Artillery")]
        ARTILLERY = 50,
        [Description("RTS")]
        RTS = 51,
        [Description("Tower Defense")]
        TOWER_DEFENSE = 52,
        [Description("Turn Based Strategy")]
        TURN_BASED_STRATEGY = 53,
        [Description("Wargame")]
        WARGAME = 54,
        [Description("MOBA")]
        MOBA = 55,
        [Description("Fighting")]
        FIGHTING = 56,
        [Description("Puzzle")]
        PUZZLE = 57,
        [Description("Card Game")]
        CARD_GAME = 58,
        [Description("Education")]
        EDUCATION = 59,
        [Description("Fitness")]
        FITNESS = 60,
        [Description("Gambling")]
        GAMBLING = 61,
        [Description("Music Rhythm")]
        MUSIC_RHYTHM = 62,
        [Description("Party Mini Game")]
        PARTY_MINI_GAME = 63,
        [Description("Pinball")]
        PINBALL = 64,
        [Description("Trivia Board Game ")]
        TRIVIA_BOARD_GAME = 65
    }
}
