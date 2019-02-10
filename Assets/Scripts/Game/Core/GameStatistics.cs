using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Utils.Pool;

namespace Game.Core
{
    public class GameStatistics
    {
        [ConfigVar(name = "show.fps", defaultValue = "0", description = "Set to value > 0 to see fps stats.")]
        public static ConfigVar showFps;

        [ConfigVar(name = "show.compact_stats", defaultValue = "1",
            description = "Set to value > 0 to see compact stats.")]
        public static ConfigVar showCompactStats;

        private const int NoFrames = 128;

        public int rtt;
        private int _lastWorldTick;
        private Color _fpsColor = new Color(0.5f, 0.0f, 0.2f);
        private Color[] _histColor = {Color.green, Color.grey};
        private readonly Stopwatch _stopWatch;
        private long _lastFrameTicks;
        private float _frameDurationMs;
        private float[] _frameDurationArray;
        private float[][] _ticksPerFrame;
        private readonly long _frequencyMs;
        private string _graphicsDeviceName;

        private RecorderEntry[] _recorderList =
        {
        };

        public GameStatistics()
        {
            _frequencyMs = Stopwatch.Frequency / 1000;
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            _lastFrameTicks = _stopWatch.ElapsedTicks;
            _frameDurationArray = new float[NoFrames];
            _ticksPerFrame = new[] {new float[NoFrames], new float[NoFrames]};

            _graphicsDeviceName = SystemInfo.graphicsDeviceName;

            foreach (RecorderEntry recorderEntry in _recorderList)
            {
                Sampler sampler = Sampler.Get(recorderEntry.name);
                if (sampler != null)
                {
                    recorderEntry.recorder = sampler.GetRecorder();
                }
            }

            Console.Console.AddCommand("show.profilers", CmdShowProfilers, "Show available profilers.");
        }

        private void CmdShowProfilers(string[] args)
        {
            var names = SimpleObjectPool.Pop<List<string>>();
            Sampler.GetNames(names);

            string search = args.Length > 0 ? args[0].ToLower() : null;
            foreach (string name in names)
            {
                if (search == null || name.ToLower().Contains(search))
                {
                    Console.Console.Write(name);
                }
            }

            names.Clear();
            SimpleObjectPool.Push(names);
        }

        private void SnapTime()
        {
            long now = _stopWatch.ElapsedTicks;
            long duration = now - _lastFrameTicks;
            _lastFrameTicks = now;
            float d = (float) duration / _frequencyMs;
            _frameDurationMs = math.lerp(_frameDurationMs, d, 0.1f);
            _frameDurationArray[Time.frameCount % _frameDurationArray.Length] = d;
        }

        private void RecordTimers()
        {
        }

        private class RecorderEntry
        {
            public string name;
            public float time;
            public int count;
            public float avgTime;
            public float avgCount;
            public float accTime;
            public int accCount;
            public Recorder recorder;
        }
    }
}