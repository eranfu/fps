#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;

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

    [Serializable]
    public class WeakAssetReference
    {
        [SerializeField] private string guid = "";

#if UNITY_EDITOR
        public T LoadAsset<T>() where T : UnityEngine.Object
        {
            string path = AssetDatabase.GUIDToAssetPath(this.guid);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
#endif
    }

    [Serializable]
    public class WeakBase
    {
        [SerializeField] private string guid = "";
    }

    // Derive from this to create a typed weak asset reference
    public class Weak<T> : WeakBase
    {
    }
}