using Game.Core;
using UnityEngine;

namespace Render
{
    public static class RenderSettings
    {
        private static ConfigVar _rResolution;

        public static void Init()
        {
            Console.Console.AddCommand("r_resolution", CmdResolution, "Display or set resolution, e.g. 1280x720");
            Console.Console.AddCommand("r_quality", CmdQuality, "Set the render quality");
            Console.Console.AddCommand("r_maxqueue", CmdMaxQueue, "Max queued frames");
            Console.Console.AddCommand("r_srpbatching", CmdSrpBatching, "Use 0 or 1 to disable or enable SRP batching");

            if (_rResolution.Value == "")
                _rResolution.Value = Screen.currentResolution.width + "x" + Screen.currentResolution.height + "@" + Screen.currentResolution.refreshRate;
        }

        private static void CmdSrpBatching(string[] args)
        {
            throw new System.NotImplementedException();
        }

        private static void CmdMaxQueue(string[] args)
        {
            throw new System.NotImplementedException();
        }

        private static void CmdQuality(string[] args)
        {
            throw new System.NotImplementedException();
        }

        private static void CmdResolution(string[] args)
        {
            throw new System.NotImplementedException();
        }
    }
}