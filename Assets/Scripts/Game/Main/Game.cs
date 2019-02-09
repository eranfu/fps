using Game.Core;
using Unity.Mathematics;
using UnityEngine;
using Utils.EnumeratedArray;
using Utils.WeakAssetReference;

namespace Game.Main
{
    public struct GameTime
    {
        private int _tickRate;

        /// <summary>
        /// Number of ticks per second.
        /// </summary>
        public int TickRate
        {
            get => _tickRate;
            set
            {
                _tickRate = value;
                TickInterval = 1.0f / _tickRate;
            }
        }

        /// <summary>
        /// Duration between ticks in seconds at current tick rate.
        /// </summary>
        public float TickInterval { get; private set; }

        /// <summary>
        /// Current tick.
        /// </summary>
        public int Tick { get; private set; }

        /// <summary>
        /// Duration of current tick.
        /// </summary>
        public float TickDuration { get; private set; }

        public float TickDurationAsFraction => TickDuration / TickInterval;

        public GameTime(int tickRate)
        {
            _tickRate = tickRate;
            TickInterval = 1.0f / _tickRate;
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
            return (Tick - tick) * TickInterval + TickDuration;
        }

        public void AddDuration(float duration)
        {
            TickDuration += duration;
            var deltaTicks = (int) math.floor(TickDuration * TickRate);
            Tick += deltaTicks;
            TickDuration %= TickInterval;
        }

        public static float GetDuration(GameTime start, GameTime end)
        {
            return (end.Tick * end.TickInterval + end.TickDuration) -
                   (start.Tick * start.TickInterval + start.TickDuration);
        }
    }

    [DefaultExecutionOrder(-1000)]
    public class Game : MonoBehaviour
    {
        public delegate void UpdateDelegate();

        public static double frameTime;

        public WeakAssetReference movableBoxPrototype;

        [EnumeratedArray(typeof(GameColor))] public Color[] gameColor;

        public GameStatistics GameStatistics { get; private set; }

        public enum GameColor
        {
            Friend,
            Enemy
        }
    }
}