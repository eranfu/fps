using System.Collections.Generic;
using Game.Main;

namespace Game.Entity
{
    public class GameWorld
    {
        public static readonly List<GameWorld> Worlds = new List<GameWorld>();
        public GameTime worldTime;
        public int lastServerTick;
        public double nextTickTime;
        public float frameDuration { get; set; }
    }
}