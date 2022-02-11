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
        private IslandMesh _mesh;
        private Biome _biome;
        private Vector2Int _cell;
        private CardinalDirection _rotation;

        /// <summary>
        /// Position within the world grid
        /// </summary>
        public Vector2Int Cell => _cell;

        /// <summary>
        /// Biome the island was spawned from
        /// </summary>
        public Biome Biome => _biome;

        /// <summary>
        /// Island mesh that this island represents
        /// </summary>
        public IslandMesh Mesh => _mesh;

        /// <summary>
        /// Return the cardinal direction the island is rotated to face
        /// </summary>
        public CardinalDirection Rotation => _rotation;

        /// <summary>
        /// Return the island grid array index
        /// </summary>
        public int GridIndex => IslandGrid.CellToIndex (Cell);

        /// <summary>
        /// Get / Set the cardinal direction that that island faces
        /// </summary>
        public CardinalDirection Direction
        {
            get => _rotation;
            set
            {
                _rotation = value;
                transform.localRotation = Quaternion.Euler(0, 90.0f * (int)_rotation, 0);
            }
        }


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

        /// <summary>
        /// Bind the island to an island mesh
        /// </summary>
        public void Bind (IslandMesh islandMesh, Vector2Int cell, Biome biome, CardinalDirection rotation)
        {
            _cell = cell;
            _biome = biome;
            _mesh = islandMesh;
            _rotation = rotation;

            var meshRenderer = GetComponent<MeshRenderer>();
            var meshFilter = GetComponent<MeshFilter>();
            var meshCollider = GetComponent<MeshCollider>();

            var mesh = islandMesh.GetComponent<MeshFilter>().sharedMesh;
            meshFilter.sharedMesh = mesh;
            meshCollider.sharedMesh = mesh;
            meshRenderer.sharedMaterials = new Material[] { _biome.Material, meshRenderer.sharedMaterials[1] };

            transform.position = IslandGrid.CellToWorld(cell);
            transform.rotation = transform.localRotation = Quaternion.Euler(0, 90.0f * (int)_rotation, 0);
        }

        /// <summary>
        /// Convert a world position to a tile within the island
        /// </summary>
        public IslandTile WorldToTile(Vector3 position)
        {
            position = transform.InverseTransformPoint(position);
            var cell = new Vector2Int((int)position.x + IslandMesh.GridCenter, -(int)position.z + IslandMesh.GridCenter);
            return _mesh.GetTile(cell);
        }
    }
}
