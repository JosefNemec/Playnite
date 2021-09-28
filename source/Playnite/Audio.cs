using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Audio
{
    public class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;

        public WaveFormat WaveFormat => reader.WaveFormat;

        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed)
            {
                return 0;
            }

            int read = reader.Read(buffer, offset, count);
            if (reader.Position + read >= reader.Length)
            {
                if (reader is IDisposable d)
                {
                    d.Dispose();
                }

                isDisposed = true;
            }

            return read;
        }
    }

    public class LoopStream : WaveStream
    {
        readonly WaveStream sourceStream;

        public LoopStream(WaveStream source)
        {
            sourceStream = source;
        }

        public override WaveFormat WaveFormat
        {
            get { return sourceStream.WaveFormat; }
        }

        public override long Length
        {
            get { return long.MaxValue / 32; }
        }

        public override long Position
        {
            get
            {
                return sourceStream.Position;
            }
            set
            {
                sourceStream.Position = value;
            }
        }

        public override bool HasData(int count)
        {
            return true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;
            while (read < count)
            {
                int required = count - read;
                int readThisTime = sourceStream.Read(buffer, offset + read, required);
                if (readThisTime < required)
                {
                    sourceStream.Position = 0;
                }

                if (sourceStream.Position >= sourceStream.Length)
                {
                    sourceStream.Position = 0;
                }
                read += readThisTime;
            }
            return read;
        }

        protected override void Dispose(bool disposing)
        {
            sourceStream.Dispose();
            base.Dispose(disposing);
        }
    }

    public class CachedSound
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }

        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                WaveFormat = audioFileReader.WaveFormat;
                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }

                AudioData = wholeFile.ToArray();
            }
        }
    }

    public class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        public long Position { get; private set; }
        public WaveFormat WaveFormat => cachedSound.WaveFormat;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = cachedSound.AudioData.Length - Position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(cachedSound.AudioData, Position, buffer, offset, samplesToCopy);
            Position += samplesToCopy;
            return (int)samplesToCopy;
        }
    }

    public class PlayingSound : IDisposable
    {
        private readonly AudioFileReader reader;
        private readonly LoopStream loop;
        public readonly ISampleProvider SampleProvider;

        public float Volume
        {
            get => reader.Volume;
            set { reader.Volume = value; }
        }

        public PlayingSound(AudioFileReader reader, LoopStream loop, ISampleProvider sampleProvider): this(reader)
        {
            this.loop = loop;
            SampleProvider = sampleProvider;
        }

        public PlayingSound(AudioFileReader reader)
        {
            this.reader = reader;
            SampleProvider = reader;
        }

        public void Dispose()
        {
            if (loop != null)
            {
                loop.Dispose();
            }
            else
            {
                reader.Dispose();
            }
        }
    }

    public enum AudioInterfaceApi
    {
        WaveOut,
        DirectSound,
        WASAPI
    }

    public class AudioPlaybackEngine : IDisposable
    {
        private readonly IWavePlayer outputDevice;
        private readonly MixingSampleProvider mixer;
        private bool isDisposed = false;

        public AudioPlaybackEngine(AudioInterfaceApi api, int sampleRate = 44100, int channelCount = 2)
        {
            switch (api)
            {
                case AudioInterfaceApi.WaveOut:
                    outputDevice = new WaveOut();
                    break;
                case AudioInterfaceApi.DirectSound:
                    outputDevice = new DirectSoundOut();
                    break;
                case AudioInterfaceApi.WASAPI:
                    outputDevice = new WasapiOut();
                    break;
            }

            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            mixer.ReadFully = true;
            outputDevice.Init(mixer);
            outputDevice.Play();
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input;
            }
            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }

            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        public void PausePlayback()
        {
            if (isDisposed)
            {
                return;
            }

            outputDevice.Pause();
        }

        public void ResumePlayback()
        {
            if (isDisposed)
            {
                return;
            }

            outputDevice.Play();
        }

        public VolumeSampleProvider PlaySound(CachedSound sound, float volume = 1f, bool loop = false)
        {
            if (isDisposed)
            {
                return null;
            }

            if (sound == null)
            {
                return null;
            }

            var mixInput = new CachedSoundSampleProvider(sound);
            var ss = new VolumeSampleProvider(mixInput);
            ss.Volume = volume * volume;
            AddMixerInput(ss);
            return ss;
        }

        public PlayingSound PlaySound(string fileName, float volume = 1f, bool loop = false)
        {
            if (isDisposed)
            {
                return null;
            }

            if (!File.Exists(fileName))
            {
                return null;
            }

            var input = new AudioFileReader(fileName) { Volume = volume };
            if (loop)
            {
                var looper = new LoopStream(input);
                var sample = looper.ToSampleProvider();
                AddMixerInput(sample);
                return new PlayingSound(input, looper, sample);
            }
            else
            {
                var autoDispose = new AutoDisposeFileReader(input);
                AddMixerInput(autoDispose);
                return new PlayingSound(input);
            }
        }

        public void StopPlayback(PlayingSound sound)
        {
            if (isDisposed)
            {
                return;
            }

            mixer.RemoveMixerInput(sound.SampleProvider);
        }

        public void Dispose()
        {
            isDisposed = true;
            outputDevice.Dispose();
        }
    }
}
