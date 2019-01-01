using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Game.Core
{
    public class GameStatistics
    {
        [ConfigVar(Name = "show.fps", DefaultValue = "0", Description = "Set to value > 0 to see fps stats.")]
        public static ConfigVar showFps;

        [ConfigVar(Name = "show.compact_stats", DefaultValue = "1",
            Description = "Set to value > 0 to see compact stats.")]
        public static ConfigVar showCompactStats;

        private const int NoFrames = 128;

        public int rtt;
        private Color _fpsColor = new Color(0.5f, 0.0f, 0.2f);
        private Color[] _histColor = {Color.green, Color.grey};
        private readonly Stopwatch _stopWatch;
        private long _lastFrameTicks;
        private float _frameDurationMs;
        private float[] _frameTimes;
        private float[][] _ticksPerFrame;
        private long _frequencyMs;
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
            _frameTimes = new float[NoFrames];
            _ticksPerFrame = new[] {new float[NoFrames], new float[NoFrames]};
            _graphicsDeviceName = SystemInfo.graphicsDeviceName;

            foreach (var recorderEntry in _recorderList)
            {
                var sampler = Sampler.Get(recorderEntry.name);
                if (sampler != null)
                {
                    recorderEntry.recorder = sampler.GetRecorder();
                }
            }


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