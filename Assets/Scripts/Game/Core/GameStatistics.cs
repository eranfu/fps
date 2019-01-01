using System.Diagnostics;
using UnityEngine;

namespace Game.Core
{
    public class GameStatistics
    {
        public int rtt;
        private readonly int _noFrames = 128;
        private Color _fpsColor = new Color(0.5f, 0.0f, 0.2f);
        private Color[] _histColor = {Color.green, Color.grey};
        private Stopwatch _stopWatch;
        private long _lastFrameTicks;
        private float _frameDurationMs;
        private float[] _frameTimes;
        private float[][] _ticksPerFrame;
        private long _frequencyMs;
        private string _graphicsDeviceName;

        public GameStatistics()
        {
        }
    }
}