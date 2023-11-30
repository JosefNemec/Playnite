using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;
using static SDL2.SDL_mixer;

namespace Playnite.Audio
{
    public class AudioEngine : IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public static readonly string[] SupportedFileTypes = new string[] { "wav", "ogg", "mp3", "flac" };
        public static readonly string SupportedFileTypesRegex = string.Join("|", SupportedFileTypes);

        public AudioEngine()
        {
            if (Mix_OpenAudio(44_100, MIX_DEFAULT_FORMAT, 2, 2048) < 0)
            {
                logger.Error("Failed to open SDL2 audio device:");
                logger.Error(Mix_GetError());
                return;
            }
        }

        public static int GetVolume(float floatVolume)
        {
            floatVolume *= floatVolume;
            // lerp settings float 0 -> 1 range to SDL's 0 -> 128 range
            var res = (int)Math.Ceiling((0f * (1f - floatVolume)) + (128f * floatVolume));
            return res;
        }

        public void Dispose()
        {
            Mix_Quit();
        }
    }
}
