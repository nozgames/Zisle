using NoZ.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Manages a running game and all if its state
    /// </summary>
    public class Game : NetworkBehaviour
    {
        [SerializeField] private GameObject _clientIslandPrefab = null;
        [SerializeField] private GameObject _clientBridgePrefab = null;

        private struct SpawnPoint
        {
            public Biome Biome;
            public Vector2Int Cell;
            public Quaternion Rotation;
        }

        public struct PathNode
        {
            public static readonly PathNode Invalid = new PathNode { IsPath = false, To = new Vector2Int(int.MaxValue, int.MaxValue) };

            public bool IsValid => To != Invalid.To;

            public bool IsPath;
            public Vector2Int To;
        }

        private NetworkVariable<IslandVisibility> _islandVisibility = new NetworkVariable<IslandVisibility> ();

        private IslandCell[] _cells = null;
        private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
        private Island[] _islands = new Island[IslandGrid.IndexMax];
        private PathNode[] _pathMap = new PathNode[TileGrid.IndexMax];
        private List<Actor>[] _actorsByType;

        private LinkedList<Actor> _actors = new LinkedList<Actor> ();

        public bool HasIslands { get; private set; }

        public int Level { get; private set; }

        public int WaveEnemyRemainingCount { get; set; }

        public int WaveEnemyCount { get; set; } = 5;

        public int WaveDelay { get; set; } = 10;

        public int WaveEnemyDelay { get; set; } = 1;

        public int WaveInitialDelay { get; set; } = 5;

        public static Game Instance => GameManager.Instance.Game;

        // TODO: if we have the island cells stored here then whenever a player connects and the GAme
        //       spawns on their client, they can request the cells be send

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            name = "Game";

            Debug.Log("Game Spawned");

            // If we are on a client then request the islands from the server.
            if (IsHost)
            {
                GameEvent<ActorDiedEvent>.OnRaised += OnActorDied;

                GenerateIslands(GameManager.Instance.Options);
                Debug.Log("Islands Generated as Host");
                HasIslands = true;

                StartCoroutine(SpawnWaves());
            }
            else
            {
                Debug.Log("Requesting cells");
                QueryIslandsServerRpc(NetworkManager.LocalClientId);
            }

            GameManager.Instance.Game = this;

            // Prepare the actors by type lists
            _actorsByType = new List<Actor>[Actor.ActorTypeCount];
            for (int i = 0; i < Actor.ActorTypeCount; i++)
                _actorsByType[i] = new List<Actor>();

            GameEvent<ActorSpawnEvent>.OnRaised += OnActorSpawn;
            GameEvent<ActorDespawnEvent>.OnRaised += OnActorDespawn;

            _islandVisibility.OnValueChanged += OnIslandVisibilityChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            GameEvent<ActorDiedEvent>.OnRaised -= OnActorDied;
        }

        private void OnActorDied(object sender, ActorDiedEvent evt)
        {
            var actor = sender as Actor;
            if(actor.Definition.ActorType == ActorType.Enemy)
                WaveEnemyRemainingCount--;
        }

        private void OnActorSpawn(object sender, ActorSpawnEvent evt)
        {
            var actor = sender as Actor;
            _actorsByType[(int)actor.Definition.ActorType].Add(actor);
            _actors.AddLast(actor.Node);
        }

        private void OnActorDespawn(object sender, ActorDespawnEvent evt)
        {
            var actor = sender as Actor;
            _actorsByType[(int)actor.Definition.ActorType].Remove(actor);
            _actors.Remove(actor.Node);
        }

        [ServerRpc(RequireOwnership = false)]
        private void QueryIslandsServerRpc(ulong clientId)
        {
            Debug.Log($"Sending Cells tp {clientId}");
            SendIslandsToClientRpc(_cells); // , new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } });
        }
            
        [ClientRpc]
        private void SendIslandsToClientRpc(IslandCell[] cells) // , ClientRpcParams rcpParams)
        {
            Debug.Log("SendIslandsToClientRpc");

            if (IsHost)
                return;

            // Save the cells
            _cells = cells;

            SpawnIslandMeshes();

            HasIslands = true;
        }

        /// <summary>
        /// Convert a world position to a tile cell
        /// </summary>
        public static Vector2Int WorldToTileCell(Vector3 position) => new Vector2Int((int)position.x, (int)position.z);

        /// <summary>
        /// Convert a tile cell to a world position
        /// </summary>
        public static Vector3 TileCellToWorld(Vector2Int cell) => new Vector3((int)cell.x, 0.0f, (int)cell.y);

        /// <summary>
        /// Return the island at the given island cell
        /// </summary>
        public Island CellToIsland(Vector2Int cell) => _islands[IslandGrid.CellToIndex(cell)];


        private void SpawnIslandMeshes ()
        {
            // Spawn the islands in simplified form
            // Spawn the islands on the host will all prefabs
            foreach (var cell in _cells)
            {
                var biome = NetworkScriptableObject<Biome>.Get(cell.BiomeId);
                if (biome == null)
                    throw new System.InvalidProgramException($"Biome id {cell.BiomeId} not found");

                // Instatiate the island itself
                var island = Instantiate(_clientIslandPrefab, transform).GetComponent<Island>();
                _islands[IslandGrid.CellToIndex(cell.Position)] = island;
                island.Bind(biome.Islands[cell.IslandIndex], cell.Position, biome, cell.Rotation, cell.Position != IslandGrid.CenterCell ? CellToIsland(cell.To) : null);

                // Spawn client bridge for navmesh
                if (cell.Position != IslandGrid.CenterCell && biome.Bridge != null)
                {
                    var from = IslandGrid.CellToWorld(cell.Position);
                    var to = IslandGrid.CellToWorld(cell.To);
                    Instantiate(_clientBridgePrefab, (from + to) * 0.5f, Quaternion.LookRotation((to - from).normalized, Vector3.up), transform);
                }
            }

            GetComponent<NavMeshSurface>().BuildNavMesh();

            foreach(var island in _islands.Where(i => i != null))
                island.gameObject.SetActive(false);
        }

        private void GenerateIslands (GameOptions options)
        {
            if (!NetworkManager.Singleton.IsHost)
                throw new System.InvalidOperationException("Generate islands can only be called on the host");
            
            // Generate the cells
            _cells = (new WorldGenerator()).Generate(options.ToGeneratorOptions());

            SpawnIslandMeshes();

            // Create bridge definitions for all islands
            foreach (var cell in _cells)
            {
                var biome = NetworkScriptableObject<Biome>.Get(cell.BiomeId);
                if (biome == null)
                    throw new System.InvalidProgramException($"Biome id {cell.BiomeId} not found");

                var island = CellToIsland(cell.Position);
                var islandPrefab = biome.Islands[cell.IslandIndex];

                foreach (var spawner in islandPrefab.GetComponentsInChildren<ActorSpawner>())
                    if (spawner.gameObject != islandPrefab)
                        island.AddSpawner(spawner);

                // Create bridge links 
                if(cell.Position != IslandGrid.CenterCell)
                { 
                    var from = IslandGrid.CellToWorld(cell.Position);
                    var to = IslandGrid.CellToWorld(cell.To);
                    var bridgePos = (from + to) * 0.5f;
                    var bridgeRot = Quaternion.LookRotation((to - from).normalized, Vector3.up);
                    CellToIsland(cell.To).AddBridge(biome.Bridge, bridgePos, bridgeRot, island);
                }
            }            
        }

        public void Play()
        {
            CellToIsland(IslandGrid.CenterCell).RiseFromTheDeep();
        }

        public void BuildNavMesh()
        {
            GetComponent<NavMeshSurface>().BuildNavMesh();
        }

        public void ShowIsland (Island island)
        {
            _islandVisibility.Value = _islandVisibility.Value.SetVisible(island.GridIndex, true);
        }

        private void OnIslandVisibilityChanged (IslandVisibility oldValue, IslandVisibility newValue)
        {
            // Determine which islands are visible
            for(int i=0; i<IslandGrid.IndexMax; i++)
            {
                if(oldValue.IsVisible(i) != newValue.IsVisible(i))
                    _islands[i].RiseFromTheDeep();
            }
        }

        public void AddSpawnPoint (Biome biome, Vector3 position, Quaternion rotation)
        {
            RemoveSpawnPoint(position);

            _spawnPoints.Add(new SpawnPoint { Biome = biome, Cell = WorldToTileCell(position), Rotation = rotation });
        }

        public void RemoveSpawnPoint (Vector3 position)
        {
            var cell = WorldToTileCell(position);
            for(int i=0; i<_spawnPoints.Count; i++)
                if(_spawnPoints[i].Cell == cell)
                {
                    _spawnPoints[i] = _spawnPoints[_spawnPoints.Count - 1];
                    _spawnPoints.RemoveAt(_spawnPoints.Count - 1);
                    return;
                }
        }

        public void SpawnEnemy ()
        {
            if (_spawnPoints.Count == 0)
                return;

            var spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Count)];
            var enemyDef = spawnPoint.Biome.ChooseRandomEnemy();
            if (null == enemyDef)
                return;

            WaveEnemyRemainingCount++;

            enemyDef.Spawn(TileCellToWorld(spawnPoint.Cell), spawnPoint.Rotation);
        }

        private IEnumerator SpawnWaves ()
        {
            yield return new WaitForSeconds(WaveInitialDelay);

            while (IsSpawned)
            {
                for(int i=0; i< WaveEnemyCount; i++)
                {
                    SpawnEnemy();
                    yield return new WaitForSeconds(WaveEnemyDelay);
                }

                while (WaveEnemyRemainingCount > 0)
                    yield return null;


                yield return new WaitForSeconds(WaveDelay);
            }
        }

        /// <summary>
        /// Convert a world coordinate into an island, null if there is no island at that world coordinate
        /// </summary>
        public Island WorldToIsland (Vector3 position)
        {
            var cell = IslandGrid.WorldToCell(position);
            if (!IslandGrid.IsValidCell(cell))
                return null; ;

            return _islands[IslandGrid.CellToIndex(cell)];
        }

        /// <summary>
        /// Convert a world coordinate to a tile on an island
        /// </summary>
        public IslandTile WorldToTile(Vector3 position)
        {
            var island = WorldToIsland(position);
            if(island == null)
                return IslandTile.None;

            return island.WorldToTile(position);
        }

        public void SetPathMap (Vector2Int from, Vector2Int to)
        {
            _pathMap[TileGrid.CellToIndex(from)] = new PathNode { IsPath = true, To = to };
        }

        public PathNode WorldToPathNode(Vector3 position) =>
            _pathMap[TileGrid.WorldToIndex(position)];

        public Vector3 WorldToPathMapDestination (Vector3 position) =>
            TileGrid.CellToWorld(WorldToPathNode(position).To);

        /// <summary>
        /// Find all actors in the <paramref name="radius"/> from <paramref name="position"/> that match the <paramref name="mask"/>
        /// </summary>
        public int FindActors (Vector3 position, float radius, ActorTypeMask mask, List<Actor> results)
        {
            results.Clear();

            var radiusSqr = radius * radius;
            for(var node = _actors.First; node != null; node = node.Next)
            {
                var actor = node.Value;
                if ((actor.TypeMask & mask) == 0)
                    continue;

                var distSqr = actor.DistanceToSqr(position);
                if (distSqr > radiusSqr)
                    continue;

                results.Add(actor);
            }

            return results.Count;
        }

        /// <summary>
        /// Find the actor within <paramref name="radius"/> of <paramref name="position"/> that matches the <paramref name="mask"/>
        /// </summary>
        public Actor FindClosestActor (Vector3 position, float radius, ActorTypeMask mask)
        {
            var radiusSqr = radius * radius;
            var bestDistSqr = float.MaxValue;
            var bestActor = (Actor)null;
            for (var node = _actors.First; node != null; node = node.Next)
            {
                var actor = node.Value;
                if ((actor.TypeMask & mask) == 0)
                    continue;

                var distSqr = actor.DistanceToSqr(position);
                if (distSqr > radiusSqr || distSqr > bestDistSqr)
                    continue;

                bestDistSqr = distSqr;
                bestActor = actor;
            }

            return bestActor;
        }
    }
}
