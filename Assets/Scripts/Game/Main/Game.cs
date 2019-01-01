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

        public static double frameTime;

        public WeakAssetReference movableBoxPrototype;

        [EnumeratedArray(typeof(GameColor))] public Color[] gameColor;

        public enum GameColor
        {
            Friend,
            Enemy
        }
    }
}