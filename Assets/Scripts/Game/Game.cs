using NoZ.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.Collections;
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

        private NetworkVariable<IslandVisibility> _islandVisibility = new NetworkVariable<IslandVisibility> ();

        private IslandCell[] _cells = null;
        private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();

        private Island[] _islands = new Island[WorldGenerator.GridIndexMax];

        public bool HasIslands { get; private set; }

        public int Level { get; private set; }

        public int WaveEnemyRemainingCount { get; set; }

        public int WaveEnemyCount { get; set; } = 1;

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

            _islandVisibility.OnValueChanged += OnIslandVisibilityChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            GameEvent<ActorDiedEvent>.OnRaised -= OnActorDied;
        }

        private void OnActorDied(object sender, ActorDiedEvent evt)
        {
            if(sender is Enemy)
            {
                WaveEnemyRemainingCount--;
            }
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
        public Vector2Int WorldToTileCell(Vector3 position) => new Vector2Int((int)position.x, (int)position.z);

        /// <summary>
        /// Convert a tile cell to a world position
        /// </summary>
        public Vector3 TileCellToWorld(Vector2Int cell) => new Vector3((int)cell.x, 0.0f, (int)cell.y);

        /// <summary>
        /// Return the island at the given island cell
        /// </summary>
        public Island GetIsland(Vector2Int cell) => _islands[WorldGenerator.GetCellIndex(cell)];

        private void SpawnIslandMeshes ()
        {
            // Spawn the islands in simplified form
            // Spawn the islands on the host will all prefabs
            foreach (var cell in _cells)
            {
                var biome = NetworkScriptableObject<Biome>.Get(cell.BiomeId);
                if (biome == null)
                    throw new System.InvalidProgramException($"Biome id {cell.BiomeId} not found");

                var islandPrefab = biome.Islands[cell.IslandIndex];

                // Instatiate the island itself
                var island = Instantiate(
                    _clientIslandPrefab,
                    WorldGenerator.CellToWorld(cell.Position),
                    Quaternion.Euler(0, 90 * cell.Rotation, 0), transform).GetComponent<Island>();

                var mesh = islandPrefab.GetComponent<MeshFilter>().sharedMesh;
                var meshRenderer = island.GetComponent<MeshRenderer>();
                island.GetComponent<MeshFilter>().sharedMesh = mesh;
                island.GetComponent<MeshCollider>().sharedMesh = mesh;
                meshRenderer.sharedMaterials = new Material[] { biome.Material, meshRenderer.sharedMaterials[1] };
                island.Cell = cell.Position;
                island.Biome = biome;

                _islands[WorldGenerator.GetCellIndex(cell.Position)] = island;

                // Bridge navmeshes
                if (cell.Position != Vector2Int.zero && biome.Bridge != null)
                {
                    var from = WorldGenerator.CellToWorld(cell.Position);
                    var to = WorldGenerator.CellToWorld(cell.To);
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

                var island = GetIsland(cell.Position);
                var islandPrefab = biome.Islands[cell.IslandIndex];

                foreach (var spawner in islandPrefab.GetComponentsInChildren<ActorSpawner>())
                    if (spawner.gameObject != islandPrefab)
                        island.AddSpawner(spawner);

                // Create bridge links 
                if(cell.Position != Vector2Int.zero)
                { 
                    var from = WorldGenerator.CellToWorld(cell.Position);
                    var to = WorldGenerator.CellToWorld(cell.To);
                    var bridgePos = (from + to) * 0.5f;
                    var bridgeRot = Quaternion.LookRotation((to - from).normalized, Vector3.up);
                    GetIsland(cell.To).AddBridge(biome.Bridge, bridgePos, bridgeRot, island);
                }
            }            
        }

        public void Play()
        {
            GetIsland(Vector2Int.zero).RiseFromTheDeep();
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
            for(int i=0; i<WorldGenerator.GridIndexMax; i++)
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
            while (IsSpawned)
            {
                yield return new WaitForSeconds(1.0f);

                SpawnEnemy();

                while (WaveEnemyRemainingCount >= WaveEnemyCount)
                    yield return null;
            }
        }
    }
}
