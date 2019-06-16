using Game.Core;
using Game.Main;
using System.Collections.Generic;
using Game.Modules.ReplicatedEntity;
using Unity.Entities;
using UnityEngine;

namespace Game.Entity
{
    public struct DespawningEntity : IComponentData
    {
    }

    [DisableAutoCreation]
    public class DestroyDespawning : ComponentSystem
    {
        private ComponentGroup _group;

        protected override void OnCreateManager()
        {
            base.OnCreateManager();
            _group = GetComponentGroup(typeof(DespawningEntity));
        }

        protected override void OnUpdate()
        {
            EntityArray entityArray = _group.GetEntityArray();
            for (var i = 0; i < entityArray.Length; i++)
            {
                PostUpdateCommands.DestroyEntity(entityArray[i]);
            }
        }
    }

    public class GameWorld
    {
        [ConfigVar(
            name = "gameobjecthierarchy",
            description = "Should game object be organized in a game object hierarchy.",
            defaultValue = "0")]
        private static ConfigVar _gameObjectHierarchy;

        public static readonly List<GameWorld> Worlds = new List<GameWorld>();

        public GameTime worldTime;
        public int lastServerTick;
        public double nextTickTime;
        private World _ecsWorld;
        private readonly EntityManager _entityManager;
        private readonly DestroyDespawning _destroyDespawningSystem;
        private readonly List<GameObject> _dynamicEntities = new List<GameObject>();
        private readonly List<GameObject> _despawnRequests = new List<GameObject>();
        private readonly List<Unity.Entities.Entity> _despawnEntityRequests = new List<Unity.Entities.Entity>();
        public float frameDuration { get; set; }
        public GameObject SceneRoot { get; }

        public GameWorld(string name = "world")
        {
            if (_gameObjectHierarchy.IntValue == 1)
            {
                SceneRoot = new GameObject(name);
                Object.DontDestroyOnLoad(SceneRoot);
            }

#if UNITY_EDITOR
            _ecsWorld = World.Active ?? new World(name);
#else
            Debug.Assert(World.Active == null);
            _ecsWorld = new World(name);
#endif

            World.Active = _ecsWorld;
            _entityManager = _ecsWorld.GetOrCreateManager<EntityManager>();

            Debug.Assert(_entityManager.IsCreated);

            worldTime.TicksPerSecond = 60;
            nextTickTime = GameRoot.frameTime;

            Worlds.Add(this);

            _destroyDespawningSystem = _ecsWorld.GetOrCreateManager<DestroyDespawning>();
        }

        public void Shutdown()
        {
            foreach (GameObject entity in _dynamicEntities)
            {
                if (_despawnRequests.Contains(entity))
                    continue;
#if UNITY_EDITOR
                if (entity == null)
                    continue;
                var gameObjectEntity = entity.GetComponent<GameObjectEntity>();
                if (gameObjectEntity != null && !_entityManager.Exists(gameObjectEntity.Entity))
                    continue;
#endif
                RequestDespawn(entity);
            }

            ProcessDespawn();

            Worlds.Remove(this);

            if (_ecsWorld.IsCreated)
            {
                _ecsWorld.Dispose();
                _ecsWorld = null;
                World.Active = null;
            }

            Object.Destroy(SceneRoot);
        }

        public void RegisterSceneEntities()
        {
            Object.FindObjectsOfType<ReplicatedEntity>();
        }

        private void ProcessDespawn()
        {
            foreach (GameObject request in _despawnRequests)
            {
                _dynamicEntities.Remove(request);
                Object.Destroy(request);
            }

            _despawnRequests.Clear();

            foreach (Unity.Entities.Entity request in _despawnEntityRequests)
            {
                _entityManager.DestroyEntity(request);
            }

            _despawnEntityRequests.Clear();

            _destroyDespawningSystem.Update();
        }

        private void RequestDespawn(GameObject entity)
        {
            if (_despawnRequests.Contains(entity))
            {
                Debug.Assert(false, $"Trying to request despawning of same gameObject({entity.name}) multiple times");
                return;
            }

            var gameObjectEntity = entity.GetComponent<GameObjectEntity>();
            if (gameObjectEntity != null)
            {
                _entityManager.AddComponent(gameObjectEntity.Entity, typeof(DespawningEntity));
            }

            _despawnRequests.Add(entity);
        }
    }
}