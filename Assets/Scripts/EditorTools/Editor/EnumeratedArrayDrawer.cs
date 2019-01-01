using Game.Main;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Assets.Scripts.EditorTools.Editor
{
    [CustomPropertyDrawer(typeof(EnumeratedArrayAttribute))]
    public class EnumeratedArrayDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool ok = int.TryParse(property.propertyPath.AfterLast("[").BeforeFirst("]"), out int idx);
            string[] names = ((EnumeratedArrayAttribute) attribute).names;
            string name = ok && idx >= 0 && idx < names.Length ? names[idx] : $"Unknown ({idx})";
            EditorGUI.PropertyField(position, property, new GUIContent(name));
        }
    }
}