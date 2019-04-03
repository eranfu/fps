using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Audio;
using Boo.Lang;
using Build;
using Game.Core;
using Game.Frontend;
using Game.Systems;
using GameConsole;
using Networking.SQP;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
using Utils;
using Utils.EnumeratedArray;
using Utils.WeakAssetReference;
using Console = GameConsole.Console;
using Debug = UnityEngine.Debug;
using DebugOverlay = Utils.DebugOverlay.DebugOverlay;
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

        private readonly List<Camera> _cameraStack = new List<Camera>();
        private readonly List<IGameLoop> _gameLoops = new List<IGameLoop>();
        private readonly List<string[]> _requestedGameLoopArguments = new List<string[]>();
        private readonly List<Type> _requestedGameLoopTypes = new List<Type>();

        private ClientFrontend _clientFrontend;

        private Stopwatch _clock;
        private GameConfiguration _config;
        private DebugOverlay _debugOverlay;
        private AutoExposure _exposure;
        private InputSystem _inputSystem;
        private bool _isHeadless;
        private LevelManager _levelManager;

        private ISoundSystem _soundSystem;
        private SQPClient _sqpClient;
        private long _stopWatchFrequency;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private SoundBank defaultBank;

        [EnumeratedArray(typeof(GameColor))] public Color[] gameColor;

        public WeakAssetReference movableBoxPrototype;
        public static ISoundSystem SoundSystem => game._soundSystem;

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

            string sqpPort = ArgumentForOption(commandLineArgs, "-query_port");
            if (sqpPort != null)
            {
                Console.EnqueueCommandNoHistory($"server.sqp_port {sqpPort}");
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

            if (_isHeadless)
            {
                _soundSystem = new SoundSystemNull();
            }
            else
            {
                _soundSystem = new SoundSystem();
                _soundSystem.Init(audioMixer);
                _soundSystem.MountBank(defaultBank);

                GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/ClientFrontend"));
                DontDestroyOnLoad(go);
                _clientFrontend = go.GetComponentInChildren<ClientFrontend>();
            }

            _sqpClient = new SQPClient();

            GameDebug.Log("FPS initialized");
#if UNITY_EDITOR
            GameDebug.Log("Build type: editor");
#elif DEVELOPMENT_BUILD
            GameDebug.Log("Build type: development");
#else
            GameDebug.Log("Build type: release");
#endif
            GameDebug.Log($"Build id: {BuildId}");
            GameDebug.Log($"Cwd: {Directory.GetCurrentDirectory()}");

            SimpleBundleManager.Init();
            GameDebug.Log("SimpleBundleManager initialized");

            _levelManager = new LevelManager();
            _levelManager.Init();
            GameDebug.Log("LevelManager initialized");

            _inputSystem = new InputSystem();
            GameDebug.Log("InputSystem initialized");

            _config = Instantiate((GameConfiguration) Resources.Load("GameConfiguration"));
            GameDebug.Log("Loaded game config");

            // Game loops
            Console.AddCommand("preview", CmdPreview, "Start preview mode");
            Console.AddCommand("serve", CmdServe, "Start server listening");
            Console.AddCommand("client", CmdClient, "client: Enter client mode.");
            Console.AddCommand("thinclient", CmdThinClient, "client: Enter thin client mode.");
            Console.AddCommand("boot", CmdBoot, "Go back to boot loop.");
            Console.AddCommand("connect", CmdConnect, "connect <ip>: Connect to server on ip (default: localhost)");

            Console.AddCommand("menu", CmdMenu, "Show the main menu.");
            Console.AddCommand("load", CmdLoad, "LoadLevel.");
        }

        private void CmdLoad(string[] args)
        {
            LoadLevel(args[0]);
            Console.SetOpen(false);
        }

        private void LoadLevel(string levelName)
        {
            if (!_levelManager.CanLoadLevel(levelName))
            {
                GameDebug.LogError($"Cannot load level: {levelName}");
                return;
            }

            _levelManager.LoadLevel(levelName);
        }

        private void CmdMenu(string[] args)
        {
            var fadeDuration = 0.0f;
            var show = ClientFrontend.MenuShowing.Main;
            if (args.Length >= 1)
            {
                if (args[0] == "0")
                    show = ClientFrontend.MenuShowing.None;
                else if (args[0] == "2")
                    show = ClientFrontend.MenuShowing.InGame;
            }

            if (args.Length >= 2)
            {
                float.TryParse(
                    args[1], NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out fadeDuration);
            }

            _clientFrontend.ShowMenu(show, fadeDuration);
            Console.SetOpen(false);
        }

        private void CmdConnect(string[] args)
        {
            if (_gameLoops.Count == 0)
            {
                RequestGameLoop(typeof(ClientGameLoop), args);
                Console.pendingCommandsWaitForFrames = 1;
                return;
            }

            var clientGameLoop = GetGameLoop<ClientGameLoop>();
            if (clientGameLoop != null)
            {
                clientGameLoop.CmdConnect(args);
                return;
            }

            var thinClientGameLoop = GetGameLoop<ThinClientGameLoop>();
            if (thinClientGameLoop != null)
            {
                thinClientGameLoop.CmdConnect(args);
                return;
            }

            GameDebug.Log("Cannot connect from current game mode.");
        }

        private static T GetGameLoop<T>() where T : class
        {
            if (game == null)
            {
                return null;
            }

            for (var i = 0; i < game._gameLoops.Count; i++)
            {
                if (game._gameLoops[i] is T result)
                {
                    return result;
                }
            }

            return null;
        }

        private void CmdBoot(string[] args)
        {
            _clientFrontend.ShowMenu(ClientFrontend.MenuShowing.None);
            _levelManager.UnloadLevel();
            ShutdownGameLoops();
            Console.pendingCommandsWaitForFrames = 1;
            Console.SetOpen(true);
        }

        private void ShutdownGameLoops()
        {
            for (var i = 0; i < _gameLoops.Count; i++)
            {
                IGameLoop gameLoop = _gameLoops[i];
                gameLoop.Shutdown();
            }

            _gameLoops.Clear();
        }

        private void CmdThinClient(string[] args)
        {
            RequestGameLoop(typeof(ThinClientGameLoop), args);
            Console.pendingCommandsWaitForFrames = 1;
        }

        private void CmdClient(string[] args)
        {
            RequestGameLoop(typeof(ClientGameLoop), args);
            Console.pendingCommandsWaitForFrames = 1;
        }

        private void CmdServe(string[] args)
        {
            RequestGameLoop(typeof(ServerGameLoop), args);
            Console.pendingCommandsWaitForFrames = 1;
        }

        private void CmdPreview(string[] args)
        {
            RequestGameLoop(typeof(PreviewGameLoop), args);
            Console.pendingCommandsWaitForFrames = 1;
        }

        private void RequestGameLoop(Type loopType, string[] args)
        {
            Debug.Assert(typeof(IGameLoop).IsAssignableFrom(loopType));
            _requestedGameLoopTypes.Add(loopType);
            _requestedGameLoopArguments.Add(args);
            GameDebug.Log($"Game loop {loopType} requested");
        }

        private static string ArgumentForOption(string[] args, string option)
        {
            int idx = Array.IndexOf(args, option);
            if (idx < 0)
                return null;
            return idx < args.Length - 1 ? args[idx + 1] : "";
        }

        public Camera TopCamera()
        {
            int count = _cameraStack.Count;
            return count == 0 ? null : _cameraStack[count - 1];
        }

        public void BlackFade(bool enabled)
        {
            if (_exposure != null)
            {
                _exposure.active = enabled;
            }
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