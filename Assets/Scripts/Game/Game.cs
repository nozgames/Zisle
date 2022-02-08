using System.Collections;
using System.Collections.Generic;
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

        private IslandCell[] _cells = null;

        public bool HasIslands { get; private set; }

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
                GenerateIslands(GameManager.Instance.Options);
                Debug.Log("Islands Generated as Host");
                HasIslands = true;
            }
            else
            {
                Debug.Log("Requesting cells");
                QueryIslandsServerRpc(NetworkManager.LocalClientId);
            }

            GameManager.Instance.Game = this;
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
                    CellToWorld(cell.Position),
                    Quaternion.Euler(0, 90 * cell.Rotation, 0), transform);
                island.GetComponent<MeshFilter>().sharedMesh = islandPrefab.GetComponent<MeshFilter>().sharedMesh;
                island.GetComponent<MeshRenderer>().material = biome.Material;
            }

            GetComponent<NavMeshSurface>().BuildNavMesh();

            Debug.Log("Islands Generated as Client");

            HasIslands = true;
        }

        private void GenerateIslands (GameOptions options)
        {
            if (!NetworkManager.Singleton.IsHost)
                throw new System.InvalidOperationException("Generate islands can only be called on the host");
            
            // Generate the cells
            _cells = (new IslandGenerator()).Generate(options);

            // Spawn the islands on the host will all prefabs
            foreach(var cell in _cells)
            {
                var biome = NetworkScriptableObject<Biome>.Get(cell.BiomeId);
                if (biome == null)
                    throw new System.InvalidProgramException($"Biome id {cell.BiomeId} not found");

                var islandPrefab = biome.Islands[cell.IslandIndex];

                // Instatiate the island itself
                var island = Instantiate(
                    islandPrefab, 
                    CellToWorld(cell.Position), 
                    Quaternion.Euler(0, 90 * cell.Rotation, 0), transform).GetComponent<Island>();
                island.GetComponent<MeshRenderer>().material = biome.Material;

                // Spawn all network objects on the island
                foreach (var netobj in island.GetComponentsInChildren<NetworkObject>())
                    netobj.Spawn();

                // Spawn bridges
                if (cell.Position != Vector2Int.zero && biome.Bridge != null)
                {
                    var from = CellToWorld(cell.Position);
                    var to = CellToWorld(cell.To);
                    Instantiate(biome.Bridge, (from + to) * 0.5f, Quaternion.LookRotation((to-from).normalized, Vector3.up), transform);
                }
            }

            GetComponent<NavMeshSurface>().BuildNavMesh();
        }

        public void BuildNavMesh()
        {
            GetComponent<NavMeshSurface>().BuildNavMesh();
        }

        private Vector3 CellToWorld(Vector2Int position) =>
            new Vector3(position.x * 12.0f, 0, position.y * -12.0f);
    }
}
