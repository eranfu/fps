using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Game.Core
{
    public enum LevelState
    {
        Loading,
        Loaded
    }

    public struct LevelLayer
    {
        public AsyncOperation loadOperation;
    }

    public class Level
    {
        public LevelState state;
        public string name;
        public List<LevelLayer> layers = new List<LevelLayer>(10);
    }

    public class LevelManager
    {
        private Level _currentLevel;
        private static readonly string[] LayerNames = {"background", "gameplay"};

        public void Init()
        {
        }

        public void UnloadLevel()
        {
            if (_currentLevel == null)
            {
                return;
            }

            if (_currentLevel.state == LevelState.Loading)
            {
                throw new NotImplementedException("TODO: Implement unload during loading.");
            }

            SceneManager.LoadScene(1);

            SimpleBundleManager.ReleaseLevelAssetBundle(_currentLevel.name);
            _currentLevel = null;
        }

        public bool CanLoadLevel(string levelName)
        {
            AssetBundle bundle = SimpleBundleManager.LoadLevelAssetBundle(levelName);
            return bundle != null;
        }

        public bool LoadLevel(string levelName)
        {
            if (_currentLevel != null)
                UnloadLevel();
            Main.GameRoot.gameRoot.TopCamera().enabled = false;
            Main.GameRoot.gameRoot.BlackFade(true);

            AssetBundle bundle = SimpleBundleManager.LoadLevelAssetBundle(levelName);
            if (bundle == null)
            {
                GameDebug.LogWarning($"Could not load asset bundle for scene: {levelName}");
                return false;
            }

            var allScenePaths = new List<string>(bundle.GetAllScenePaths());
            if (allScenePaths.Count < 1)
            {
                GameDebug.LogWarning($"No scene in asset bundle.");
                return false;
            }

            string mainScenePath = allScenePaths.Find(path => path.ToLower().EndsWith("_main.unity"));
            var useLayers = true;
            if (mainScenePath == null)
            {
                useLayers = false;
                mainScenePath = allScenePaths[0];
            }

            GameDebug.Log($"Loading {mainScenePath}");
            AsyncOperation mainLoadOperation = SceneManager.LoadSceneAsync(mainScenePath, LoadSceneMode.Single);
            if (mainLoadOperation == null)
            {
                GameDebug.LogWarning($"Failed to load level: {levelName}");
                return false;
            }

            var newLevel = new Level {name = levelName};
            _currentLevel = newLevel;
            _currentLevel.layers.Add(new LevelLayer {loadOperation = mainLoadOperation});

            if (!useLayers)
                return true;

            for (var i = 0; i < LayerNames.Length; i++)
            {
                string layerName = LayerNames[i];
                string path = allScenePaths.Find(l => l.ToLower().EndsWith($"{layerName}.unity"));
                if (path == null)
                    continue;

                GameDebug.Log($"+Loading {path}");
                AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
                if (loadSceneAsync != null)
                {
                    _currentLevel.layers.Add(new LevelLayer {loadOperation = loadSceneAsync});
                }
                else
                {
                    GameDebug.LogWarning($"Unable to load level layer: {path}");
                }
            }

            return true;
        }

        public bool IsLoadingLevel()
        {
            return _currentLevel != null && _currentLevel.state == LevelState.Loading;
        }
    }
}