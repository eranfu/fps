using Game.Core;
using GameConsole;
using UnityEngine;

namespace Render
{
    public static class RenderSettings
    {
        private static ConfigVar _rResolution;

        public static void Init()
        {
            Console.AddCommand("r_resolution", CmdResolution, "Display or set resolution, e.g. 1280x720");
            Console.AddCommand("r_quality", CmdQuality, "Set the render quality");
            Console.AddCommand("r_maxqueue", CmdMaxQueue, "Max queued frames");
            Console.AddCommand("r_srpbatching", CmdSrpBatching, "Use 0 or 1 to disable or enable SRP batching");

            if (_rResolution.Value == "")
                _rResolution.Value = Screen.currentResolution.width + "x" + Screen.currentResolution.height + "@" +
                                     Screen.currentResolution.refreshRate;
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
            if (args.Length > 0)
            {
                string wantedResolution = args[0];
                var res = new int[3];
                var a = 0;
                for (var i = 0; i < wantedResolution.Length; i++)
                {
                    char c = wantedResolution[i];
                    if (c >= '0' && c <= '9')
                    {
                        res[a] = res[a] * 10 + (c - '0');
                    }
                    else if (c == 'x' || c == '@')
                    {
                        ++a;
                    }
                    else
                    {
                        break;
                    }
                }

                int width = res[0];
                int height = res[1];
                int refreshRate = res[2] > 0 ? res[2] : Screen.currentResolution.refreshRate;
                if (width > 100 && height > 100)
                {
                    Screen.SetResolution(width, height, Screen.fullScreenMode, refreshRate);
                }
                else
                {
                    Console.Write(
                        "Invalid resolution. Use <width>x<height>[@<refresh>] with width and height > 100");
                }
            }

            Console.Write("Resolution supported by monitor: ");
            Resolution[] resolutions = Screen.resolutions;
            for (var i = 0; i < resolutions.Length; i++)
            {
                Console.Write($"{resolutions[i].width}x{resolutions[i].height}@{resolutions[i].refreshRate}");
            }

            Console.Write($"Full screen mode: {(int) Screen.fullScreenMode}({Screen.fullScreenMode})");
            Console.Write($"Current window resolution: {Screen.width}x{Screen.height}");
            Console.Write(
                $"Current screen resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height}@{Screen.currentResolution.refreshRate}");
        }

        public static void UpdateCameraSettings(Camera cam)
        {
            UpdateAAFlags(cam);
        }
    }
}