using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class SoundSystemNull : ISoundSystem
    {
        public void Init(AudioMixer audioMixer)
        {
        }

        public void MountBank(SoundBank bank)
        {
        }

        public SoundHandle Play(SoundDef soundDef)
        {
            return default;
        }

        public void SetCurrentListener(AudioListener audioListener)
        {
        }
    }
}