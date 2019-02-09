using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils.WeakAssetReference.Editor
{
    public static class WeakAssetReferenceDrawerHelper
    {
        public static void GuidField(Type type, Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty guid = property.FindPropertyRelative("guid");
            string assetPath = AssetDatabase.GUIDToAssetPath(guid.stringValue);
            Object obj = AssetDatabase.LoadAssetAtPath(assetPath, type);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive),
                new GUIContent($"{label.text}({guid.stringValue})"));
            Object newObj = EditorGUI.ObjectField(position, obj, type, false);
            if (newObj != obj)
            {
                if (newObj != null)
                {
                    assetPath = AssetDatabase.GetAssetPath(newObj);
                    property.stringValue = AssetDatabase.AssetPathToGUID(assetPath);
                }
                else
                {
                    property.stringValue = "";
                }
            }
        }
    }
}