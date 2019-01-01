using System;
using UnityEngine;
using Utils.WeakAssetReference;

namespace Game.Main
{
    public class EnumeratedArrayAttribute : PropertyAttribute
    {
        public readonly string[] names;

        public EnumeratedArrayAttribute(Type type)
        {
            this.names = Enum.GetNames(type);
        }
    }

    [DefaultExecutionOrder(-1000)]
    public class Game : MonoBehaviour
    {
        public delegate void UpdateDelegate();

        public WeakAssetReference movableBoxPrototype;

        public enum GameColor
        {
            Friend,
            Enemy
        }

        [EnumeratedArray(typeof(GameColor))] public Color[] gameColor;
    }
}