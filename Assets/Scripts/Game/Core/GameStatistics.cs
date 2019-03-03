using System.Collections.Generic;
using System.Diagnostics;
using Game.Entity;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Utils.DebugOverlay;
using Utils.Pool;

namespace Game.Core
{
    public class GameStatistics
    {
        private const int NoFrames = 128;
        private const int AverageFrameCount = 64;

        [ConfigVar(name = "show.fps", defaultValue = "0", description = "Set to value > 0 to see fps stats.")]
        public static ConfigVar showFps;

        [ConfigVar(name = "show.compact_stats", defaultValue = "1",
            description = "Set to value > 0 to see compact stats.")]
        public static ConfigVar showCompactStats;

        private static readonly long TicksPerMillisecond = Stopwatch.Frequency / 1000;
        private static readonly string GraphicsDeviceName = SystemInfo.graphicsDeviceName;
        private static readonly Color FpsColor = new Color(0.5f, 0.0f, 0.2f);
        private static readonly Color[] HistColor = {Color.green, Color.grey};

        private readonly float[] _frameDurationArray;

        private readonly RecorderEntry[] _recorderList =
        {
            new RecorderEntry {name = "RenderLoop.Draw"},
            new RecorderEntry {name = "Shadows.Draw"},
            new RecorderEntry {name = "RenderLoopNewBatcher.Draw"},
            new RecorderEntry {name = "ShadowLoopNewBatcher.Draw"},
            new RecorderEntry {name = "RenderLoopDevice.Idle"},
            new RecorderEntry {name = "StaticBatchDraw.Count"}
        };

        private readonly Stopwatch _stopWatch;

        private readonly float[][] _ticksPerFrame;
        private int _frameCount = 0;
        private float _frameDurationMs;
        private long _lastFrameTicks;
        private int _lastWorldTick;

        public int rtt;

        public GameStatistics()
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            _lastFrameTicks = _stopWatch.ElapsedTicks;
            _frameDurationArray = new float[NoFrames];
            _ticksPerFrame = new[] {new float[NoFrames], new float[NoFrames]};

            foreach (RecorderEntry recorderEntry in _recorderList)
            {
                Sampler sampler = Sampler.Get(recorderEntry.name);
                if (sampler != null)
                    recorderEntry.recorder = sampler.GetRecorder();
            }

            Console.Console.AddCommand("show.profilers", CmdShowProfilers, "Show available profilers.");
        }

        private void CmdShowProfilers(string[] args)
        {
            var names = Pools.SimpleObject.Pop<List<string>>();
            Sampler.GetNames(names);

            string search = args.Length > 0 ? args[0].ToLower() : null;
            foreach (string name in names)
                if (search == null || name.ToLower().Contains(search))
                    Console.Console.Write(name);

            names.Clear();
            Pools.SimpleObject.Push(names);
        }

        private void SnapTime()
        {
            long now = _stopWatch.ElapsedTicks;
            long deltaTicks = now - _lastFrameTicks;
            _lastFrameTicks = now;
            float frameDuration = (float) deltaTicks / TicksPerMillisecond;
            _frameDurationMs = math.lerp(_frameDurationMs, frameDuration, 0.1f);
            _frameDurationArray[Time.frameCount % _frameDurationArray.Length] = frameDuration;
        }

        private void RecordTimers()
        {
            if (GameWorld.Worlds.Count > 0)
            {
                GameWorld world = GameWorld.Worlds[0];
                int ticks = world.worldTime.Tick - _lastWorldTick;
                int l = Time.frameCount % _ticksPerFrame[0].Length;
                _ticksPerFrame[0][l] = 1000.0f * world.worldTime.SecondsPerTick * ticks;
                _lastWorldTick = world.worldTime.Tick;
                double lastTickTime = world.nextTickTime - world.worldTime.SecondsPerTick;
                _ticksPerFrame[1][l] = (float) (1000.0 * (Main.Game.frameTime - lastTickTime));
            }

            // get timing & update average accumulators
            foreach (RecorderEntry t in _recorderList)
            {
                t.time = t.recorder.elapsedNanoseconds / 1000000.0f;
                t.count = t.recorder.sampleBlockCount;
                t.accTime += t.time;
                t.accCount += t.count;
            }

            _frameCount++;
            // time to time, update average values & reset accumulators
            if (_frameCount >= AverageFrameCount)
            {
                foreach (RecorderEntry t in _recorderList)
                {
                    t.avgTime = t.accTime * (1.0f / AverageFrameCount);
                    t.avgCount = t.accCount * (1.0f / AverageFrameCount);
                    t.accTime = 0.0f;
                    t.accCount = 0;
                }

                _frameCount = 0;
            }
        }

        public void TickLateUpdate()
        {
            SnapTime();
            if (showCompactStats.IntValue > 0) DrawCompactStats();

            if (showFps.IntValue > 0)
            {
                RecordTimers();
                DrawFPS();
            }
        }

        private void DrawFPS()
        {
            DebugOverlay.WriteString(0, 1,
                $"{Mathf.RoundToInt(1000.0f / _frameDurationMs)} FPS ({_frameDurationMs:##.##} ms)");
            float minDuration = float.MaxValue;
            float maxDuration = float.MinValue;
            float sum = 0;
            for (var i = 0; i < NoFrames; i++)
            {
                float frameDuration = _frameDurationArray[i];
                sum += frameDuration;
                if (frameDuration < minDuration) minDuration = frameDuration;
                if (frameDuration > maxDuration) maxDuration = frameDuration;
            }

            DebugOverlay.WriteString(Color.green, 0, 2, $"{minDuration:##.##}");
            DebugOverlay.WriteString(Color.grey, 6, 2, $"{sum / NoFrames:##.##}");
            DebugOverlay.WriteString(Color.red, 12, 2, $"{maxDuration:##.##}");

            DebugOverlay.WriteString(0, 3, $"Frame #: {Time.frameCount}");

            DebugOverlay.WriteString(0, 4, GraphicsDeviceName);


            var y = 6;
            for (var i = 0; i < _recorderList.Length; i++)
                DebugOverlay.WriteString(
                    0, y++,
                    $"{_recorderList[i].avgTime:##.##}ms (*{_recorderList[i].avgCount:##})  ({_recorderList[i].time:##.##}ms *{_recorderList[i].count:##})  {_recorderList[i].name}");

            if (showFps.IntValue < 3)
                return;

            y++;
            // Start at framecount+1 so the one we have just recorded will be the last
            DebugOverlay.DrawHistogram(0, y, 20, 2, _frameDurationArray, Time.frameCount + 1, FpsColor, 20.0f);
            DebugOverlay.DrawHistogram(0, y + 2, 20, 2, _ticksPerFrame, Time.frameCount + 1, HistColor, 3.0f * 16.0f);

            DebugOverlay.DrawGraph(0, y + 6, 40, 2, _frameDurationArray, Time.frameCount + 1, FpsColor, 20.0f);

            if (GameWorld.Worlds.Count > 0)
            {
                GameWorld world = GameWorld.Worlds[0];
                DebugOverlay.WriteString(0, y + 8, $"Tick: {1000.0f * world.worldTime.SecondsPerTick:##.#}");
            }
        }

        private void DrawCompactStats()
        {
            char[] buf = Pools.Buffer.Pop<char>(256);
            DebugOverlay.AddQuadAbsolute(0, 0, 60, 14, '\0', new Vector4(1.0f, 1.0f, 1.0f, 0.2f));
            int c = StringFormatter.Write(ref buf, 0, "FPS:{0}", Mathf.RoundToInt(1000.0f / _frameDurationMs));
            DebugOverlay.WriteAbsolute(2, 2, 8.0f, buf, c);

            DebugOverlay.AddQuadAbsolute(62, 0, 60, 14, '\0', new Vector4(1.0f, 1.0f, 0.0f, 0.2f));
            c = rtt > 0
                ? StringFormatter.Write(ref buf, 0, "RTT:{0}", rtt)
                : StringFormatter.Write(ref buf, 0, "RTT:---");
            DebugOverlay.WriteAbsolute(64, 2, 8.0f, buf, c);
            Pools.Buffer.Push(buf);
        }

        private class RecorderEntry
        {
            public int accCount;
            public float accTime;
            public float avgCount;
            public float avgTime;
            public int count;
            public string name;
            public Recorder recorder;
            public float time;
        }
    }
}