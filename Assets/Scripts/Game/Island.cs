using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public class Island : MonoBehaviour
    {
        private struct BridgeDef
        {
            public Bridge Prefab;
            public Vector3 Position;
            public Quaternion Rotation;
            public Island To;
        }

        private List<BridgeDef> _bridges = new List<BridgeDef>();

        private List<ActorSpawner> _spawners = new List<ActorSpawner>();

        /// <summary>
        /// Position within the world grid
        /// </summary>
        public Vector2Int Cell { get; set; }

        /// <summary>
        /// Biome the island was spawned from
        /// </summary>
        public Biome Biome { get; set; }

        /// <summary>
        /// Return the index on the island grid
        /// </summary>
        public int GridIndex => WorldGenerator.GetCellIndex(Cell);

        public void AddBridge(Bridge prefab, Vector3 position, Quaternion rotation, Island to)
        {
            _bridges.Add(new BridgeDef { Prefab = prefab, Position = position, Rotation = rotation, To = to });
        }

        public void AddSpawner (ActorSpawner spawner)
        {
            var spawned = Instantiate(spawner.gameObject, spawner.transform.localPosition, spawner.transform.localRotation, transform).GetComponent<ActorSpawner>();
            if (null == spawned)
                return;

            _spawners.Add(spawned);
        }

        public void RiseFromTheDeep()
        {
            gameObject.SetActive(true);
            SpawnActors();
            SpawnBridges();
        }

        private void SpawnActors ()
        {
            foreach (var spawner in _spawners)
                spawner.Spawn();
        }

        private void SpawnBridges()
        {
            foreach (var def in _bridges)
            {
                var bridge = Instantiate(def.Prefab, def.Position, def.Rotation).GetComponent<Bridge>();
                bridge.Bind(from: this, to: def.To);
                bridge.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
