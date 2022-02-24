using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.VFX;
using System.Linq;

namespace NoZ.Zisle
{
    public enum IslandState
    {
        None,

        Hidden,

        Spawn,

        Active
    }

    public class Island : MonoBehaviour
    {
        [SerializeField] private CameraShakeDefinition _fallShake = null;
        [SerializeField] private AudioShader _splashSound = null;
        [SerializeField] private AudioShader _fallSound = null;
        [SerializeField] private VisualEffectAsset _splashFX = null;

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
        private IslandState _state = IslandState.None;

        public IslandState State => _state;

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

        private float _fallVelocity = 0.0f;
        private const float _fallGravity = -90.8f;

        IEnumerator Fall (List<Actor> spawnedActors)
        {
            CameraManager.Instance.StartCinematic(transform.position, 20.0f);

            yield return new WaitForSeconds(0.5f);

            _state = IslandState.Spawn;
            _fallVelocity = 0.0f;

            AudioManager.Instance.PlaySound(_fallSound, gameObject);

            var shake = false;
            while (Mathf.Abs(_fallVelocity) > 0.5f || Mathf.Abs(transform.position.y) > 0.01f)
            {
                _fallVelocity += _fallGravity * Time.deltaTime;
                transform.position += (Vector3.up * _fallVelocity * Time.deltaTime);

                if (transform.position.y < 0.0f)
                {
                    transform.position += (Vector3.up * -transform.position.y * 2);
                    _fallVelocity = -_fallVelocity * 0.25f;

                    if (!shake && _fallShake != null)
                    {
                        if (!shake && _splashSound != null)
                            AudioManager.Instance.PlaySound(_splashSound, gameObject);

                        // TODO: only need to update this island.
                        GameManager.Instance.GenerateWater(Game.Instance.transform);

                        SpawnSplash();

                        _fallShake.Shake(); //  Mathf.Abs(Mathf.Lerp(0.1f, 1.0f, _fallVelocity / 1.0f)));
                        shake = true;
                    }
                }

                yield return null;
            }

            transform.position = transform.position.ZeroY();

            // Now that the island has spawned in move all actors spawned by the island to the intro state
            foreach (var actor in spawnedActors)
                actor.State = ActorState.Intro;

            _state = IslandState.Active;

            SpawnHarvestables();

            CameraManager.Instance.StopCinematic();
        }

        /// <summary>
        /// Spawn the island.  Technically the island is already spawned but this will cause it to 
        /// fall from the sky and be traversable.
        /// </summary>
        public void Spawn()
        {
            if (_state != IslandState.Hidden)
                return;

            _state = IslandState.Spawn;

            gameObject.SetActive(true);

            SpawnBridges();

            // Spawn child objects
            var spawnedActors = new List<Actor>();
            for (int i = Mesh.transform.childCount - 1; i >= 0; i--)
            {
                var child = Mesh.transform.GetChild(i);
                var actor = child.GetComponent<Actor>();
                if(actor != null)
                {
                    if(NetworkManager.Singleton.IsHost)
                        spawnedActors.Add(actor.Definition.Spawn(transform.TransformPoint(child.localPosition), transform.rotation * child.localRotation, transform));
                }
                else
                {
                    var spawned = Instantiate(child.gameObject, transform);
                    spawned.transform.position = transform.TransformPoint(child.localPosition);
                    spawned.transform.rotation = transform.rotation * child.localRotation;
                    spawned.transform.localScale = child.localScale;
                }
            }

            transform.position += Vector3.up * 20.0f;
            StartCoroutine(Fall(spawnedActors));

            GeneratePathMap();
        }

        private void SpawnBridges()
        {
            foreach (var def in _bridges)
            {
                var bridge = Instantiate(def.Prefab, def.Position, def.Rotation, transform).GetComponent<Bridge>();
                bridge.Bind(from: this, to: def.To);
                bridge.GetComponent<NetworkObject>().Spawn();
            }
        }

        private void OnDisable()
        {
            _state = IslandState.Hidden;
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

            // Add paths from the island to the bridge and from the bridge to the next island
            var bridgeCell = WorldToCell(transform.position + nextIslandDir * (IslandMesh.GridCenter + 1));
            var nextIslandCell = WorldToCell(transform.position + nextIslandDir * (IslandMesh.GridCenter + 2));
            SetPathMap(cell, bridgeCell);
            SetPathMap(bridgeCell, nextIslandCell);
        }

        private void SetPathMap (Vector2Int from, Vector2Int to)
        {
            Game.Instance.SetPathMap(TileGrid.WorldToCell(CellToWorld(from)), TileGrid.WorldToCell(CellToWorld(to)));
        }

        private void SpawnSplash ()
        {
            for(int y = -1; y < IslandMesh.GridSize + 2; y++)
                for (int x = -1; x < IslandMesh.GridSize + 2; x++)
                {
                    var cell = new Vector2Int(x, y);
                    var tile = Mesh.GetTile(cell);
                    if (tile != IslandTile.Water && tile != IslandTile.None)
                        continue;

                    var center = IslandMesh.CellToLocal(cell);

                    for (int j=0; j<4; j++)
                    {
                        var offset = ((CardinalDirection)j).ToOffset();
                        var neighbor = cell + offset;
                        var neighborTile = Mesh.GetTile(neighbor);
                        if (neighborTile == IslandTile.Water || neighborTile == IslandTile.None)
                            continue;

                        var n = new Vector3(offset.x, 0, -offset.y);
                        var pos = center + n * 0.5f;

                        // Play the effect
                        VFXManager.Instance.Play(_splashFX, transform.TransformPoint(pos), transform.rotation * Quaternion.LookRotation(n, Vector3.up));
                    }
                }
        }

        private static Collider[] _results = new Collider[1];

        private IEnumerable<Vector2Int> FindFreeTiles (IslandTile filter)
        {
            for (int i = 0; i < IslandMesh.GridIndexMax; i++)
            {
                var tile = Mesh.GetTile(i);
                if (tile == IslandTile.None)
                    continue;
                if (filter != IslandTile.None && filter != tile)
                    continue;

                var cell = IslandMesh.IndexToCell(i);

                if (0 != Physics.OverlapBoxNonAlloc(CellToWorld(cell) + Vector3.up * 0.5f, new Vector3(0.5f, 0.4f, 0.5f), _results))
                    continue;

                yield return cell;
            }
        }

        public bool TryFindFreeTileNearCenter (IslandTile filter, out Vector3 position)
        {
            var cells = FindFreeTiles(filter).ToArray();
            if (null == cells || cells.Length == 0)
            {
                position = Vector3.zero;
                return false;
            }
                
            var cell = cells[ WeightedRandom.RandomWeightedIndex(FindFreeTiles(filter), 0, -1, (cell) => 1.0f - Mathf.Clamp(IslandMesh.CellToLocal(cell).magnitude / IslandMesh.GridCenter, 0.1f, 1.0f))];
            position = CellToWorld(cell);
            return true;
        }

        public bool TryFindFreeTile (IslandTile filter, out Vector3 position)
        {
            var cells = FindFreeTiles(filter).ToArray();
            if (null == cells || cells.Length == 0)
            {
                position = Vector3.zero;
                return false;
            }

            var cell = cells[WeightedRandom.RandomWeightedIndex(FindFreeTiles(filter), 0, -1, (cell) => 1.0f)];
            position = CellToWorld(cell);
            return true;
        }

        public Vector3 FindClosestExitPosition (Vector3 position)
        {
            var bestDist = float.MaxValue;
            var bestPosition = Vector3.zero;
            foreach(var bridge in _bridges)
            {
                var dist = (bridge.Position - position).sqrMagnitude;
                if(dist < bestDist)
                {
                    bestDist = dist;
                    bestPosition = bridge.Position;
                }
            }

            return bestPosition;
        }

        public void SpawnHarvestables ()
        {
            for(int i=0; i<5; i++)
            {                
                if(TryFindFreeTile(IslandTile.Grass, out var position))
                {
                    var def = _biome.ChooseRandomHarvestable();
                    if (null != def)
                        def.Spawn(position, Quaternion.identity, transform);
                }
            }
        }
    }
}


