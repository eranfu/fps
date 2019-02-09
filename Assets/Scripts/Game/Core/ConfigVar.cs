using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
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
        private static readonly Regex ValidateNameRe = new Regex(@"^[a-z_+-][a-z0-9_+.-]*$");

        private readonly string _name;
        private string _description;
        private readonly Flags _flags;
        public bool changed;

        private string _stringValue;
        private float _floatValue;
        private int _intValue;

        public string Value
        {
            get => _stringValue;
            set
            {
                if (_stringValue == value)
                    return;
                dirtyFlags |= _flags;
                _stringValue = value;
                if (!int.TryParse(_stringValue, out _intValue))
                    _intValue = 0;
                if (!float.TryParse(_stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out _floatValue))
                    _floatValue = 0;
                changed = true;
            }
        }

        public int IntValue => _intValue;

        private ConfigVar(string name, string description, Flags flags)
        {
            _name = name;
            _description = description;
            _flags = flags;
        }

        public static void Init()
        {
            if (_initialized)
                return;
            InjectAttributeConfigVars();
            _initialized = true;
        }

        private static void InjectAttributeConfigVars()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (!type.IsClass)
                        continue;
                    foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Static |
                                                             BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if (!fieldInfo.IsDefined(typeof(ConfigVarAttribute), false))
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
                        Debug.Assert(type.FullName != null, "type.FullName != null");
                        string name = attr.Name ?? $"{type.FullName.ToLower()}.{fieldInfo.Name.ToLower()}";
                        var var = (ConfigVar) fieldInfo.GetValue(null);
                        if (var != null)
                        {
                            Debug.LogError(
                                $"ConfigVar ({name}) should not be initialized from code; just marked with attribute");
                            continue;
                        }

                        var = new ConfigVar(name, attr.Description, attr.Flags)
                        {
                            Value = attr.DefaultValue
                        };
                        RegisterConfigVar(var);
                        fieldInfo.SetValue(null, var);
                    }
                }
            }

            dirtyFlags = Flags.None;
        }

        private static void RegisterConfigVar(ConfigVar var)
        {
            if (ConfigVars.ContainsKey(var._name))
            {
                Debug.LogError($"Trying to register ConfigVar {var._name} twice");
                return;
            }

            if (!ValidateNameRe.IsMatch(var._name))
            {
                Debug.LogError($"Trying to register ConfigVar with invalidate name: {var._name}");
                return;
            }

            ConfigVars.Add(var._name, var);
        }

        [Flags]
        public enum Flags
        {
            None = 0x0, // None
            Save = 0x1, // Causes the var to be save to settings.cfg
            Cheat = 0x2, // Consider this a cheat var. Can only be set if cheats enabled
            ServerInfo = 0x4, // These vars are sent to clients when connecting and when changed
            ClientInfo = 0x8, // These vars are sent to server when connecting and when changed
            User = 0x10, // User created variable
        }
    }
}