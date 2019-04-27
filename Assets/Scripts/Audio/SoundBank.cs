using UnityEditor;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "SoundBank", menuName = "FPS/Audio/SoundBank", order = 10000)]
    public class SoundBank : ScriptableObject
    {
        public SoundDef[] soundDefList;
        public string[] soundDefGuidList;

        public SoundDef FindByName(string soundName)
        {
            for (var i = 0; i < soundDefList.Length; i++)
            {
                SoundDef soundDef = soundDefList[i];
                if (soundDef.name == soundName)
                {
                    return soundDef;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            soundDefGuidList = new string[soundDefList.Length];
            for (var i = 0; i < soundDefList.Length; i++)
            {
                SoundDef soundDef = soundDefList[i];
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(soundDef, out string guid, out long _);
                soundDefGuidList[i] = guid;
            }
        }
#endif
    }
}