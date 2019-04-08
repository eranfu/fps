using System;
using Game.Core;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering.PostProcessing;
using Utils;
using Console = GameConsole.Console;

namespace Render
{
    public static class RenderSettings
    {
        [ConfigVar(name = "r.resolution", defaultValue = "", description = "Resolution", flags = ConfigVar.Flags.Save)]
        private static ConfigVar _rResolution;

        [ConfigVar(name = "r.aamode", defaultValue = "taa", description = "AA mode: off, fxaa, smaa, taa",
            flags = ConfigVar.Flags.Save)]
        private static ConfigVar _rAAMode;

        [ConfigVar(name = "r.aaquality", defaultValue = "high", description = "AA quality: low, medium, high",
            flags = ConfigVar.Flags.Save)]
        private static ConfigVar _rAAQuality;

        [ConfigVar(name = "r.sss", defaultValue = "1", description = "Enable subsurface scattering",
            flags = ConfigVar.Flags.Save)]
        private static ConfigVar _rSSS;

        [ConfigVar(name = "r.motionblur", defaultValue = "1", description = "Enable motion blur",
            flags = ConfigVar.Flags.Save)]
        private static ConfigVar _rMotionBlur;

        [ConfigVar(name = "r.ssao", defaultValue = "1", description = "Enable ssao", flags = ConfigVar.Flags.Save)]
        private static ConfigVar _rSSAO;

        [ConfigVar(name = "r.ssr", defaultValue = "1", description = "Enable screen space reflections",
            flags = ConfigVar.Flags.Save)]
        private static ConfigVar _rSSR;

        [ConfigVar(name = "r.roughrefraction", defaultValue = "1", description = "Enable rough refraction",
            flags = ConfigVar.Flags.Save)]
        private static ConfigVar _rRoughRefraction;

        [ConfigVar(name = "r.distortion", defaultValue = "1", description = "Enable distortion",
            flags = ConfigVar.Flags.Save)]
        private static ConfigVar _rDistortion;

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
            throw new NotImplementedException();
        }

        private static void CmdMaxQueue(string[] args)
        {
            throw new NotImplementedException();
        }

        private static void CmdQuality(string[] args)
        {
            throw new NotImplementedException();
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
            UpdateFrameSettings(cam);
        }

        private static void UpdateFrameSettings(Camera cam)
        {
            if (cam == null)
                return;
            var hdCam = cam.GetComponent<HDAdditionalCameraData>();
            if (hdCam == null)
                return;
            FrameSettings frameSettings = hdCam.GetFrameSettings();
            frameSettings.enableSubsurfaceScattering = _rSSS.IntValue > 0;
            frameSettings.enableMotionVectors = _rMotionBlur.IntValue > 0;
            frameSettings.enableObjectMotionVectors = _rMotionBlur.IntValue > 0;
            frameSettings.enableSSAO = _rSSAO.IntValue > 0;
            frameSettings.enableSSR = _rSSR.IntValue > 0;
            frameSettings.enableRoughRefraction = _rRoughRefraction.IntValue > 0;
            frameSettings.enableDistortion = _rDistortion.IntValue > 0;
        }

        private static void UpdateAAFlags(Camera cam)
        {
            if (cam == null)
            {
                return;
            }

            var postProcessLayer = cam.GetComponent<PostProcessLayer>();
            if (postProcessLayer == null)
                return;

            switch (_rAAMode.Value)
            {
                case "off":
                    postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.None;
                    break;
                case "taa":
                    postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
                    break;
                case "fxaa":
                    postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                    break;
                case "smaa":
                {
                    postProcessLayer.antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
                    switch (_rAAQuality.Value)
                    {
                        case "low":
                            postProcessLayer.subpixelMorphologicalAntialiasing.quality =
                                SubpixelMorphologicalAntialiasing.Quality.Low;
                            break;
                        case "medium":
                            postProcessLayer.subpixelMorphologicalAntialiasing.quality =
                                SubpixelMorphologicalAntialiasing.Quality.Medium;
                            break;
                        case "high":
                            postProcessLayer.subpixelMorphologicalAntialiasing.quality =
                                SubpixelMorphologicalAntialiasing.Quality.High;
                            break;
                        default:
                            GameDebug.LogWarning($"Unknown aa quality: {_rAAQuality.Value}");
                            break;
                    }

                    break;
                }
                default:
                    GameDebug.LogWarning($"Unknown aa mode: {_rAAMode.Value}");
                    break;
            }
        }
    }
}