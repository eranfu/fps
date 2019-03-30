using Boo.Lang;
using UnityEditor;
using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "SoundBank", menuName = "FPS/Audio/SoundBank", order = 10000)]
    public class SoundBank : ScriptableObject
    {
        public List<SoundDef> soundDefList;
        public List<string> soundDefGuidList;

#if UNITY_EDITOR
        private void OnValidate()
        {
            soundDefGuidList.Clear();
            foreach (SoundDef soundDef in soundDefList)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(soundDef, out string guid, out long _);
                soundDefGuidList.Add(guid);
            }
        }
#endif
    }
}