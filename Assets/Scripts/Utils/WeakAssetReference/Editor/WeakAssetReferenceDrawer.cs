using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Utils.WeakAssetReference.Editor
{
    [CustomPropertyDrawer(typeof(WeakAssetReference))]
    public class WeakAssetReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var assetTypeAttribute =
                Attribute.GetCustomAttribute(fieldInfo, typeof(AssetTypeAttribute)) as AssetTypeAttribute;
            Type assetType = assetTypeAttribute?.type ?? typeof(GameObject);

            SerializedProperty guid = property.FindPropertyRelative("guid");
            string assetPath = AssetDatabase.GUIDToAssetPath(guid.stringValue);
            Object obj = AssetDatabase.LoadAssetAtPath(assetPath, assetType);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive),
                new GUIContent(label.text + "(" + guid.stringValue + ")"));
            Object newObj = EditorGUI.ObjectField(position, obj, assetType, false);
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