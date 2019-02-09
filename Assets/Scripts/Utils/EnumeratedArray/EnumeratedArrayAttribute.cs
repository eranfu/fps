using System;
using UnityEngine;

namespace Utils.EnumeratedArray
{
    public class EnumeratedArrayAttribute : PropertyAttribute
    {
        public readonly string[] names;

        public EnumeratedArrayAttribute(Type enumType)
        {
            this.names = Enum.GetNames(enumType);
        }
    }
}