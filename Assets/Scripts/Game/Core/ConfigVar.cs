using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Game.Core
{
    public class ConfigVarAttribute : Attribute
    {
        public string Name = null;
        public string DefaultValue = "";
        public ConfigVar.Flags Flags = ConfigVar.Flags.None;
        public string Description = "";
    }

    public class ConfigVar
    {
        public static readonly Dictionary<string, ConfigVar> ConfigVars = new Dictionary<string, ConfigVar>();
        public static Flags dirtyFlags = Flags.None;
        private static bool _initialized = false;

        public static void Init()
        {
            if (_initialized)
                return;
            InjectAttributeConfigVars();
            _initialized = true;
        }

        private static void InjectAttributeConfigVars()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsClass)
                        continue;
                    foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Static |
                                                             BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (!fieldInfo.IsDefined(typeof(ConfigVar), false))
                            continue;
                        if (!fieldInfo.IsStatic)
                        {
                            Debug.LogError("Cannot use ConfigVar attribute on non-static fields.");
                            continue;
                        }

                        if (fieldInfo.FieldType != typeof(ConfigVar))
                        {
                            Debug.LogError("Cannot use ConfigVar attribute on fields not of type ConfigVar.");
                            continue;
                        }

                        var attr = fieldInfo.GetCustomAttribute<ConfigVarAttribute>(false);

                    }
                }
            }
        }

        public enum Flags
        {
            None = 0x0, // None
            Save = 0x1, // Causes the cvar to be save to settings.cfg
            Cheat = 0x2, // Consider this a cheat var. Can only be set if cheats enabled
            ServerInfo = 0x4, // These vars are sent to clients when connecting and when changed
            ClientInfo = 0x8, // These vars are sent to server when connecting and when changed
            User = 0x10, // User created variable
        }
    }
}