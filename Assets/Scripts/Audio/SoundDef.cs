using UnityEngine;

namespace Audio
{
    public enum SoundMixerGroup
    {
        Music,
        SFX,
        Menu,
        GroupCount
    }

    [CreateAssetMenu(fileName = "SoundDef", menuName = "FPS/Audio/SoundDef", order = 10000)]
    public class SoundDef : ScriptableObject
    {
        public AudioClip[] clips;
        [Range(0, 1)] public float spatialBlend = 1;
        [Range(1, 20)] public int repeatMin = 1;
        [Range(1, 20)] public int repeatMax = 1;
        [Range(-60, 0)] public float volume = -6;
        [Range(-20, 20)] public float pitchMin = 0;
        [Range(-20, 20)] public float pitchMax = 0;
        [Range(0.1f, 100.0f)] public float distMin = 1.5f;
        [Range(0.1f, 100.0f)] public float distMax = 20.0f;
        [Range(0, 10)] public int loopCount = 1;
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
        [Range(0, 10)] public float delayMin = 0;
        [Range(0, 10)] public float delayMax = 0;
        public SoundMixerGroup soundGroup;
        [Range(-1, 1)] public float panMin = 0;
        [Range(-1, 1)] public float panMax = 0;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (repeatMin > repeatMax)
                repeatMax = repeatMin;
            if (pitchMin > pitchMax)
                pitchMax = pitchMin;
            if (distMin > distMax)
                distMax = distMin;
            if (delayMin > delayMax)
                delayMax = delayMin;
            if (panMin > panMax)
                panMax = panMin;
        }
#endif
    }
}