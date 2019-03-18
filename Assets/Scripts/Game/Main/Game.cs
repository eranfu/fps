using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Build;
using Core;
using Game.Core;
using GameConsole;
using Unity.Mathematics;
using UnityEngine;
using Utils.DebugOverlay;
using Utils.EnumeratedArray;
using Utils.WeakAssetReference;
using Console = GameConsole.Console;
using Debug = UnityEngine.Debug;
using RenderSettings = Render.RenderSettings;

namespace Game.Main
{
    public struct GameTime
    {
        private int _ticksPerSecond;

        /// <summary>
        ///     Number of ticks per second.
        /// </summary>
        public int TicksPerSecond
        {
            get => _ticksPerSecond;
            set
            {
                _ticksPerSecond = value;
                SecondsPerTick = 1.0f / _ticksPerSecond;
            }
        }

        /// <summary>
        ///     Duration between ticks in seconds at current tick rate.
        /// </summary>
        public float SecondsPerTick { get; private set; }

        /// <summary>
        ///     Current tick.
        /// </summary>
        public int Tick { get; private set; }

        /// <summary>
        ///     Duration of current tick.
        /// </summary>
        public float TickDuration { get; private set; }

        public float TickDurationAsFraction => TickDuration / SecondsPerTick;

        public GameTime(int ticksPerSecond)
        {
            _ticksPerSecond = ticksPerSecond;
            SecondsPerTick = 1.0f / _ticksPerSecond;
            Tick = 1;
            TickDuration = 0;
        }

        public void SetTime(int tick, float tickDuration)
        {
            Tick = tick;
            TickDuration = tickDuration;
        }

        public float DurationSinceTick(int tick)
        {
            return (Tick - tick) * SecondsPerTick + TickDuration;
        }

        public void AddDuration(float duration)
        {
            TickDuration += duration;
            var deltaTicks = (int) math.floor(TickDuration * TicksPerSecond);
            Tick += deltaTicks;
            TickDuration %= SecondsPerTick;
        }

        public static float GetDuration(GameTime start, GameTime end)
        {
            return end.Tick * end.SecondsPerTick + end.TickDuration -
                   (start.Tick * start.SecondsPerTick + start.TickDuration);
        }
    }

    [DefaultExecutionOrder(-1000)]
    public class Game : MonoBehaviour
    {
        public delegate void UpdateDelegate();

        public enum GameColor
        {
            Friend,
            Enemy
        }

        private const string UserConfigFileName = "user.cfg";
        private const string BootConfigFileName = "boot.cfg";

        public static double frameTime;
        public static Game game;

        [ConfigVar(name = "server.tickrate", defaultValue = "60", description = "TickRate for server",
            flags = ConfigVar.Flags.ServerInfo)]
        private static ConfigVar _serverTickRate;

        private Stopwatch _clock;
        private DebugOverlay _debugOverlay;
        private bool _isHeadless;
        private long _stopWatchFrequency;

        [EnumeratedArray(typeof(GameColor))] public Color[] gameColor;

        public WeakAssetReference movableBoxPrototype;

        public string BuildId { get; private set; }

        public GameStatistics GameStatistics { get; private set; }
        public static event UpdateDelegate EndUpdateEvent;

        private void Awake()
        {
            Debug.Assert(game == null);
            DontDestroyOnLoad(gameObject);
            game = this;

            _stopWatchFrequency = Stopwatch.Frequency;
            _clock = new Stopwatch();
            _clock.Start();

            var buildInfo = FindObjectOfType<BuildInfo>();
            if (buildInfo != null)
                BuildId = buildInfo.GetBuildId();

            string[] commandLineArgs = Environment.GetCommandLineArgs();

#if UNITY_STANDALONE_LINUX
            _isHeadless = true;
#else
            _isHeadless = commandLineArgs.Contains("-batchmode");
#endif

            bool consoleRestoreFocus = commandLineArgs.Contains("-consolerestorefocus");

            if (_isHeadless)
            {
#if UNITY_STANDALONE_WIN
                string overrideTitle = ArgumentForOption(commandLineArgs, "-title");
                string consoleTitle = overrideTitle ?? $"{Application.productName} Console";
                consoleTitle = $"{consoleTitle} [{Process.GetCurrentProcess().Id}]";

                var consoleUi = new ConsoleTextWin(consoleTitle, consoleRestoreFocus);
#elif UNITY_STANDALONE_LINUX
                var consoleUi = new ConsoleTextLinux();
#else
                Debug.LogWarning("Starting without a console");
                var consoleUi = new ConsoleNullUi();
#endif

                Console.Init(consoleUi);
            }
            else
            {
                ConsoleGUI consoleUi = Instantiate(Resources.Load<ConsoleGUI>("Prefabs/ConsoleGui"));
                DontDestroyOnLoad(consoleUi);
                Console.Init(consoleUi);

                _debugOverlay = Instantiate(Resources.Load<DebugOverlay>("DebugOverlay"));
                DontDestroyOnLoad(_debugOverlay);
                _debugOverlay.Init();

                // todo debug render
//                if (RenderPipelineManager.currentPipeline is HDRenderPipeline hdPipe)
//                {
//                    hdPipe
//                }

                GameStatistics = new GameStatistics();
            }

            string logfileArg = ArgumentForOption(commandLineArgs, "-logfile");
            string engineLogFileLocation = logfileArg != null ? Path.GetDirectoryName(logfileArg) : ".";

            string logName = _isHeadless ? $"game_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}" : "game";
            GameDebug.Init(engineLogFileLocation, logName);

            ConfigVar.Init();

            // Support -port and -query_port as per player standard
            string serverPort = ArgumentForOption(commandLineArgs, "-port");
            if (serverPort != null)
            {
                Console.EnqueueCommandNoHistory($"server.port {serverPort}");
            }

            Console.EnqueueCommandNoHistory($"exec -s {UserConfigFileName}");

            Application.targetFrameRate = -1;

            if (_isHeadless)
            {
                Application.targetFrameRate = _serverTickRate.IntValue;
                QualitySettings.vSyncCount = 0;

#if !UNITY_STANDALONE_LINUX
                if (!commandLineArgs.Contains("-nographics"))
                    Debug.LogWarning("running -batchmod without -nographics");
#endif
            }
            else
            {
                RenderSettings.Init();
            }

            if (!commandLineArgs.Contains("-noboot"))
                Console.EnqueueCommandNoHistory($"exec -s {BootConfigFileName}");
        }

        private static string ArgumentForOption(string[] args, string option)
        {
            int idx = Array.IndexOf(args, option);
            if (idx < 0)
                return null;
            return idx < args.Length - 1 ? args[idx + 1] : "";
        }

        public static class Input
        {
            [Flags]
            public enum Blocker : uint
            {
                None = 0,
                Console = 1,
                Chat = 2,
                Debug = 3
            }

            private static Blocker _blocks;

            public static void SetBlock(Blocker block, bool value)
            {
                if (value)
                    _blocks |= block;
                else
                    _blocks &= ~block;
            }

            public static float GetAxisRaw(string axis)
            {
                return _blocks != Blocker.None ? 0.0f : UnityEngine.Input.GetAxisRaw(axis);
            }

            public static bool GetKeyDown(KeyCode key)
            {
                return _blocks == Blocker.None && UnityEngine.Input.GetKeyDown(key);
            }

            public static bool GetKey(KeyCode key)
            {
                return _blocks == Blocker.None && UnityEngine.Input.GetKey(key);
            }

            public static bool GetKeyUp(KeyCode key)
            {
                return _blocks == Blocker.None && UnityEngine.Input.GetKeyUp(key);
            }

            public static bool GetMouseButton(int button)
            {
                return _blocks == Blocker.None && UnityEngine.Input.GetMouseButton(button);
            }
        }

        public interface IGameLoop
        {
            bool Init(string[] args);
            void Shutdown();

            void Update();
            void FixedUpdate();
            void LateUpdate();
        }
    }
}