using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Game.Core
{
    public static class SimpleBundleManager
    {
        [ConfigVar(name = "res.runtimebundlepath", defaultValue = "AssetBundles", description = "Asset bundle folder",
            flags = ConfigVar.Flags.ServerInfo)]
        private static ConfigVar runtimeBundlePath;

        private static readonly Dictionary<string, AssetBundle> _levelBundles = new Dictionary<string, AssetBundle>();

        public const string assetBundleFolder = "AssetBundles";

        public static void Init()
        {
        }

        public static void ReleaseLevelAssetBundle(string name)
        {
            // todo ReleaseLevelAssetBundle
        }

        public static AssetBundle LoadLevelAssetBundle(string name)
        {
            string path = $"{GetRuntimeBundlePath()}/{name}";
            GameDebug.Log($"Loading {path}");
            if (!_levelBundles.TryGetValue(name, out AssetBundle bundle))
            {
                bundle = AssetBundle.LoadFromFile(path);
                if (bundle != null)
                    _levelBundles.Add(name, bundle);
            }

            return bundle;
        }

        private static string GetRuntimeBundlePath()
        {
#if UNITY_PS4
            return Application.streamingAssetsPath + "/" + assetBundleFolder;
#else
            return Application.isEditor ? $"AutoBuild/{assetBundleFolder}" : runtimeBundlePath.Value;
#endif
        }
    }
}