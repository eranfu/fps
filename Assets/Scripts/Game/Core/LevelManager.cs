using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            throw new NotImplementedException();
        }
    }
}