using NAudio.CoreAudioApi;
using NAudio.Wave;
using StreamTools.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamTools.Audio
{
    public enum AudioPlayerState { Disabled, Playing, Paused, Stopped }

    public class AudioPlayer
    {
        private MMDevice? _inDevice;
        private MMDevice _outDevice;
        private WaveOutEvent? _player;
        private AudioFileReader? _source;

        public event EventHandler<WaveFormUpdateEventArgs> WaveFormUpdate;
        public WasapiLoopbackCapture Wasapi { get; }

        public AudioPlayerState PlaybackState
        {
            get
            {
                return (_player?.PlaybackState) switch
                {
                    NAudio.Wave.PlaybackState.Stopped => AudioPlayerState.Stopped,
                    NAudio.Wave.PlaybackState.Paused => AudioPlayerState.Paused,
                    NAudio.Wave.PlaybackState.Playing => AudioPlayerState.Playing,
                    _ => AudioPlayerState.Disabled,
                };
            }
        }

        public AudioPlayer(MMDevice output)
        {
            Wasapi = new WasapiLoopbackCapture();

            _outDevice = output;
        }

        public AudioPlayer(MMDevice output, MMDevice input)
        {
            Wasapi = new WasapiLoopbackCapture();

            _outDevice = output;
            _inDevice = input;
        }

        private void Wasapi_DataAvailable(object? sender, WaveInEventArgs e)
        {
            if(_player != null)
            {
                List<float> channels = new()
                {
                    //_outDevice.AudioMeterInformation.PeakValues[0],
                    //_outDevice.AudioMeterInformation.PeakValues[1]
                };

                long position = _player.GetPosition();
                TimeSpan ts = TimeSpan.FromMilliseconds(position * 1000 / _player.OutputWaveFormat.BitsPerSample / _player.OutputWaveFormat.Channels * 8 / _player.OutputWaveFormat.SampleRate);

                WaveFormUpdate?.Invoke(this, new WaveFormUpdateEventArgs(channels.ToArray(), ts));
            }
        }

        public void Load(string filename)
        {
            _source = new AudioFileReader(filename);
            _player = new WaveOutEvent();
            _player.Init(_source);
        }

        public void Play()
        {
            Wasapi.DataAvailable += Wasapi_DataAvailable;
            Wasapi.StartRecording();
            _player?.Play();
        }

        public void Stop()
        {
            Wasapi.DataAvailable -= Wasapi_DataAvailable;
            Wasapi.StopRecording();
            _player?.Stop();
        }
    }
}
