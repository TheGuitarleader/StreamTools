using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamTools.Events
{
    public class WaveFormUpdateEventArgs : EventArgs
    {
        public float[] Channels { get; }
        public TimeSpan CurrentTime { get; }

        public WaveFormUpdateEventArgs(float[] channels, TimeSpan currentTime)
        {
            Channels = channels;
            CurrentTime = currentTime;
        }
    }
}
