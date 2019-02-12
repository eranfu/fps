using System;

namespace Build
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EditorOnlyGameObjectAttribute : Attribute
    {
    }
}