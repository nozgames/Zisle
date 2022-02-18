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
        /// Connected island that leads towards the home tile
        /// </summary>
        public Island Next { get; private set; }

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

        public void RiseFromTheDeep()
        {
            gameObject.SetActive(true);

            SpawnBridges();

            // Spawn child objects
            for (int i = Mesh.transform.childCount - 1; i >= 0; i--)
            {
                var child = Mesh.transform.GetChild(i);
                var actor = child.GetComponent<Actor>();
                if(actor != null)
                {
                    if(NetworkManager.Singleton.IsHost)
                        actor.Definition.Spawn(transform.TransformPoint(child.localPosition), transform.rotation * child.localRotation);
                }
                else
                {
                    var spawned = Instantiate(child.gameObject, transform);
                    spawned.transform.position = transform.TransformPoint(child.localPosition);
                    spawned.transform.rotation = transform.rotation * child.localRotation;
                    spawned.transform.localScale = child.localScale;
                }
            }

            GeneratePathMap();
        }

        private void SpawnBridges()
        {
            foreach (var def in _bridges)
            {
                var bridge = Instantiate(def.Prefab, def.Position, def.Rotation, Game.Instance.transform).GetComponent<Bridge>();
                bridge.Bind(from: this, to: def.To);
                bridge.GetComponent<NetworkObject>().Spawn();
            }
        }

        /// <summary>
        /// Bind the island to an island mesh
        /// </summary>
        public void Bind (IslandMesh islandMesh, Vector2Int cell, Biome biome, CardinalDirection rotation, Island next)
        {
            _cell = cell;
            _biome = biome;
            _mesh = islandMesh;
            _rotation = rotation;
            Next = next;

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
        /// Convert a world position to a cell position
        /// </summary>
        public Vector2Int WorldToCell(Vector3 position) =>
            IslandMesh.LocalToCell(transform.InverseTransformPoint(position));
        
        /// <summary>
        /// Convert a IslandMesh tile to a world coordinate
        /// </summary>
        public Vector3 CellToWorld (Vector2Int cell) =>
            transform.TransformPoint(IslandMesh.CellToLocal(cell));
        
        /// <summary>
        /// Convert a world position to a tile within the island
        /// </summary>
        public IslandTile WorldToTile(Vector3 position) =>
            _mesh.GetTile(WorldToCell(position));

        private class PathMapNode
        {
            public Vector2Int Cell;
            public Vector2Int Parent;
            public int Score;
        }

        private void GeneratePathMap ()
        {
            if (!NetworkManager.Singleton.IsHost)
                return;
                
            if (Next == null)
                return;

            var nextIslandDir = (Next.transform.position - transform.position).normalized;
            var cell = WorldToCell(transform.position + nextIslandDir * IslandMesh.GridCenter);
            var queue = new Queue<PathMapNode>();
            var nodes = new PathMapNode[IslandMesh.GridIndexMax];

            queue.Enqueue(new PathMapNode { Cell = cell, Score = 0, Parent = cell });

            while(queue.Count > 0)
            {
                var node = queue.Dequeue(); 

                // Check all four directions
                for(int i=0; i<4; i++)
                {
                    var neighborCell = node.Cell + ((CardinalDirection)i).ToOffset();
                    if (!_mesh.IsValidCell(neighborCell) || neighborCell == node.Parent || _mesh.GetTile(neighborCell) != IslandTile.Path)
                        continue;

                    var neighbor = nodes[IslandMesh.CellToIndex(neighborCell)];
                    if (neighbor == null)
                    {
                        neighbor = new PathMapNode { Cell = neighborCell, Parent = node.Cell, Score = int.MaxValue };
                        nodes[IslandMesh.CellToIndex(neighborCell)] = neighbor;
                    }

                    if (neighbor.Score < node.Score + 1)
                        continue;

                    neighbor.Score = node.Score + 1;
                    queue.Enqueue(neighbor);
                }
            }

            // All paths connected to the exit
            for(int i=0; i<IslandMesh.GridIndexMax; i++)
            {
                var node = nodes[i];
                if (node != null && node.Score > 0)
                    SetPathMap(node.Cell, node.Parent);
            }

            var bridgeCellWorld = transform.position + nextIslandDir * (IslandMesh.GridCenter + 1);
            var bridgeCell = WorldToCell(transform.position + nextIslandDir * (IslandMesh.GridCenter + 1));
            var nextIslandCellWorld = transform.position + nextIslandDir * (IslandMesh.GridCenter + 2);
            var nextIslandCell = WorldToCell(transform.position + nextIslandDir * (IslandMesh.GridCenter + 2));
            SetPathMap(cell, bridgeCell);
            SetPathMap(bridgeCell, nextIslandCell);
        }

        private void SetPathMap (Vector2Int from, Vector2Int to)
        {
            Game.Instance.SetPathMap(TileGrid.WorldToCell(CellToWorld(from)), TileGrid.WorldToCell(CellToWorld(to)));
        }
    }
}
