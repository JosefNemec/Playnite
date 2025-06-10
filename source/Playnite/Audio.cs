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

        public bool AudioClosed { get; private set; } = false;
        public bool AudioInitialized { get; private set; } = false;
        public DateTime LastAudioEvent { get; private set; } = DateTime.Now;

        public AudioEngine()
        {
            OpenAudio();
        }

        private void OpenAudio()
        {
            if (Mix_OpenAudio(44_100, MIX_DEFAULT_FORMAT, 2, 2048) < 0)
            {
                logger.Error("Failed to open SDL2 audio device:");
                logger.Error(Mix_GetError());
                AudioInitialized = false;
                AudioClosed = true;
                return;
            }

            AudioInitialized = true;
            AudioClosed = false;
        }

        public static int GetVolume(float floatVolume)
        {
            floatVolume *= floatVolume;
            // lerp settings float 0 -> 1 range to SDL's 0 -> 128 range
            var res = (int)Math.Ceiling((0f * (1f - floatVolume)) + (128f * floatVolume));
            return res;
        }

        public void PlaySound(IntPtr sound)
        {
            if (sound == IntPtr.Zero)
                return;

            if (!AudioInitialized)
                return;

            if (AudioClosed)
                OpenAudio();

            LastAudioEvent = DateTime.Now;
            try
            {
                Mix_PlayChannel(-1, sound, 0);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to play SDL sound.");
            }
        }

        public void SetSoundVolume(IntPtr sounds, float volume)
        {
            if (!AudioInitialized)
                return;

            Mix_VolumeChunk(sounds, GetVolume(volume));
        }

        public IntPtr LoadSound(string path)
        {
            if (!AudioInitialized)
                return IntPtr.Zero;

            return Mix_LoadWAV(path);
        }

        public void SetMusicVolume(float volume)
        {
            if (!AudioInitialized)
                return;

            Mix_VolumeMusic(GetVolume(volume));
        }

        public IntPtr LoadMusic(string path)
        {
            if (!AudioInitialized)
                return IntPtr.Zero;

            return Mix_LoadMUS(path);
        }

        public void PlayMusic(IntPtr music)
        {
            if (music == IntPtr.Zero)
                return;

            if (!AudioInitialized)
                return;

            if (AudioClosed)
                OpenAudio();

            LastAudioEvent = DateTime.Now;
            try
            {
                Mix_PlayMusic(music, -1);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to play SDL music.");
            }
        }

        public void PauseMusic()
        {
            if (!AudioInitialized)
                return;

            LastAudioEvent = DateTime.Now;
            Mix_PauseMusic();
        }

        public void StopMusic()
        {
            if (!AudioInitialized)
                return;

            LastAudioEvent = DateTime.Now;
            Mix_HaltMusic();
        }

        public void ResumeMusic()
        {
            if (!AudioInitialized)
                return;

            if (AudioClosed)
                OpenAudio();

            LastAudioEvent = DateTime.Now;
            Mix_ResumeMusic();
        }

        public bool GetIsMusicPlaying()
        {
            if (!AudioInitialized)
                return false;

            return Mix_PlayingMusic() == 1;
        }

        public bool GetIsMusicPaused()
        {
            if (!AudioInitialized)
                return false;

            return Mix_PausedMusic() == 1;
        }

        public void CloseAudio()
        {
            if (AudioClosed || !AudioInitialized)
                return;

            Console.WriteLine("closing audio");
            Mix_CloseAudio();
            AudioClosed = true;
        }

        public void DisposeSound(IntPtr sound)
        {
            Mix_FreeChunk(sound);
        }

        public void DisposeMusic(IntPtr music)
        {
            Mix_FreeMusic(music);
        }

        public void Dispose()
        {
            AudioInitialized = false;
            AudioClosed = true;
            Mix_Quit();
        }
    }
}
