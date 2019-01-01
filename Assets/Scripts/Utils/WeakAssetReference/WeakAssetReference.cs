#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Utils.WeakAssetReference
{
    [System.Serializable]
    public class WeakAssetReference
    {
        public string guid;

#if UNITY_EDITOR
        public T LoadAsset<T>() where T : UnityEngine.Object
        {
            string path = AssetDatabase.GUIDToAssetPath(this.guid);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif
    }
}