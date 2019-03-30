using UnityEngine;
using Utils;

namespace Audio
{
    public class SoundEmitter
    {
        public Interpolator fadeToKill;
        public bool playing;
        public int repeatCount;
        public int seqId;
        public SoundDef soundDef;
        public AudioSource source;

        public void Kill()
        {
            source.Stop();
            repeatCount = 0;
        }
    }
}