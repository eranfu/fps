#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Utils.WeakAssetReference
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AssetTypeAttribute : Attribute
    {
        public readonly Type type;

        public AssetTypeAttribute(Type type)
        {
            this.type = type;
        }
    }

    [System.Serializable]
    public class WeakAssetReference
    {
        public string guid = "";

#if UNITY_EDITOR
        public T LoadAsset<T>() where T : UnityEngine.Object
        {
            string path = AssetDatabase.GUIDToAssetPath(this.guid);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif
    }
}