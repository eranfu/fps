using UnityEngine.Audio;

namespace Audio
{
    public interface ISoundSystem
    {
        void Init(AudioMixer audioMixer);
        void MountBank(SoundBank bank);
        SoundHandle Play(SoundDef soundDef);
    }
}