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
        private Level currentLevel;
        private static readonly string[] LayerNames = {"background", "gameplay"};

        public void Init()
        {
        }

        public void UnloadLevel()
        {
            if (currentLevel == null)
            {
                return;
            }

            if (currentLevel.state == LevelState.Loading)
            {
                throw new NotImplementedException("TODO: Implement unload during loading.");
            }

            SceneManager.LoadScene(1);

            SimpleBundleManager.ReleaseLevelAssetBundle(currentLevel.name);
            currentLevel = null;
        }

        public bool CanLoadLevel(string levelName)
        {
            AssetBundle bundle = SimpleBundleManager.LoadLevelAssetBundle(levelName);
            return bundle != null;
        }

        public bool LoadLevel(string levelName)
        {
            if (currentLevel != null)
                UnloadLevel();
            Main.Game.game.TopCamera().enabled = false;
            Main.Game.game.BlackFade(true);

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
            currentLevel = newLevel;
            currentLevel.layers.Add(new LevelLayer {loadOperation = mainLoadOperation});

            if (!useLayers)
                return true;

            for (var i = 0; i < LayerNames.Length; i++)
            {
                
            }
        }
    }
}