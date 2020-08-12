
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model
{
    public class AudioManager
    {
        public enum SoundType { Hurt, Select, Walking_Dirt, Walking_Grass, Walking_Stone, Walking_Wood, Shoot }
        public enum Sound { dog_hurt_1, dog_hurt_2, select_1, select_2, walking_dirt_1, walking_dirt_2, walking_grass_1, walking_grass_2, walking_grass_3, walking_stone_1, walking_stone_2, walking_wood_1, shoot_1 }

        private AudioPlaybackEngine _audioEngine;

        private List<CachedSound> _cachedSounds;

        public AudioManager()
        {
            _audioEngine = new AudioPlaybackEngine(48000, 2);
            _cachedSounds = new List<CachedSound>();
            InitCachedSounds();
        }

        private void InitCachedSounds()
        {
            string path = "Content/soundeffects/";
            _cachedSounds.Add(new CachedSound(path + "dox hurt 1.wav"));
            _cachedSounds.Add(new CachedSound(path + "dox hurt 2.wav"));
            _cachedSounds.Add(new CachedSound(path + "select thing.wav"));
            _cachedSounds.Add(new CachedSound(path + "select thing 2.wav"));
            _cachedSounds.Add(new CachedSound(path + "walking dirt.wav"));
            _cachedSounds.Add(new CachedSound(path + "walking dirt 2.wav"));
            _cachedSounds.Add(new CachedSound(path + "walking grass.wav"));
            _cachedSounds.Add(new CachedSound(path + "walking grass 2.wav"));
            _cachedSounds.Add(new CachedSound(path + "walking grass 3.wav"));
            _cachedSounds.Add(new CachedSound(path + "walking stone.wav"));
            _cachedSounds.Add(new CachedSound(path + "walking stone 2.wav"));
            _cachedSounds.Add(new CachedSound(path + "walking wood.wav"));
        }

        public void PlaySound(SoundType sound)
        {
            switch (sound)
            {
                case SoundType.Hurt:
                    if(MainModel.Random.Next(2) < 1)
                    {
                        _audioEngine.PlaySound(_cachedSounds[(int)Sound.dog_hurt_1]);
                    }
                    else
                    {
                        _audioEngine.PlaySound(_cachedSounds[(int)Sound.dog_hurt_2]);
                    }
                    break;
                case SoundType.Walking_Dirt:
                    if(MainModel.Random.Next(2) < 1)
                    {
                        _audioEngine.PlaySound(_cachedSounds[(int)Sound.walking_dirt_1]);
                    }
                    else
                    {
                        _audioEngine.PlaySound(_cachedSounds[(int)Sound.walking_dirt_2]);
                    }
                    break;
                case SoundType.Walking_Grass:
                    int ran = MainModel.Random.Next(3);
                    if(ran < 1)
                    {
                        _audioEngine.PlaySound(_cachedSounds[(int)Sound.walking_grass_1]);
                    }
                    else if(ran < 2)
                    {
                        _audioEngine.PlaySound(_cachedSounds[(int)Sound.walking_grass_2]);
                    }
                    else
                    {
                        _audioEngine.PlaySound(_cachedSounds[(int)Sound.walking_grass_3]);
                    }
                    break;
                case SoundType.Walking_Stone:
                    if(MainModel.Random.Next(2) < 1)
                    {
                        _audioEngine.PlaySound(_cachedSounds[(int)Sound.walking_stone_1]);
                    }
                    else
                    {
                        _audioEngine.PlaySound(_cachedSounds[(int)Sound.walking_stone_2]);
                    }
                    break;
                case SoundType.Walking_Wood:
                    _audioEngine.PlaySound(_cachedSounds[(int)Sound.walking_wood_1]);
                    break;
            }
        }

        public void PlaySound(ushort blockType)
        {
            switch (blockType)
            {
                case 2:
                    PlaySound(SoundType.Walking_Dirt);
                    break;
                case 3:
                    PlaySound(SoundType.Walking_Grass);
                    break;
                case 4:
                    PlaySound(SoundType.Walking_Wood);
                    break;
                default:
                    PlaySound(SoundType.Walking_Stone);
                    break;
            }
        }
    }


    public class AudioPlaybackEngine : IDisposable
    {
        private readonly IWavePlayer outputDevice;
        private readonly MixingSampleProvider mixer;

        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            outputDevice = new WaveOutEvent();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            mixer.ReadFully = true;
            outputDevice.Init(mixer);
            outputDevice.Volume = 0.5f;
            outputDevice.Play();
        }

        public void PlaySound(string fileName)
        {
            var input = new AudioFileReader(fileName);
            AddMixerInput(new AutoDisposeFileReader(input));
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

        public void PlaySound(CachedSound sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }

        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        public void Dispose()
        {
            outputDevice.Dispose();
        }

        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
    }

    public class CachedSound
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                // TODO: could add resampling in here if required
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

    class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        private long position;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = cachedSound.AudioData.Length - position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy);
            position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }
    }

    class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed)
                return 0;
            int read = reader.Read(buffer, offset, count);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }
}
