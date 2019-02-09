using System;
using UnityEditor;
using UnityEngine;

namespace Utils.WeakAssetReference.Editor
{
    [CustomPropertyDrawer(typeof(WeakAssetReference))]
    public class WeakAssetReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var assetTypeAttribute =
                Attribute.GetCustomAttribute(fieldInfo, typeof(AssetTypeAttribute)) as AssetTypeAttribute;
            Type assetType = assetTypeAttribute?.type ?? typeof(GameObject);
            WeakAssetReferenceDrawerHelper.GuidField(assetType, position, property, label);
            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(WeakBase), true)]
    public class WeakBaseDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Type assetType = typeof(GameObject);
            Type baseType = fieldInfo.FieldType.BaseType;
            if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(Weak<>))
            {
                assetType = baseType.GetGenericArguments()[0];
            }

            WeakAssetReferenceDrawerHelper.GuidField(assetType, position, property, label);
            EditorGUI.EndProperty();
        }
    }
}