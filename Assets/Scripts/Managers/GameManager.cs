using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoZ.Zisle
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private Camera _camera = null;
        [SerializeField] private Biome[] _biomes = null;

        public Camera Camera => _camera;

        public override void Initialize()
        {
            base.Initialize();

            InputManager.Instance.onPlayerMenu += OnPlayerMenu;

            foreach (var biome in _biomes)
                biome.RegisterNetworkId();


            SpawnIsland(0, 0, 1);

            for(int i=0; i<20; i++)
            {
                var cells = GetOpenConnectableCells();
                if (cells.Count > 0)
                {
                    var cell = cells[Random.Range(0, cells.Count)];
                    SpawnIsland(cell.x, cell.y, i + 2);
                }
            }
        }

        private void OnPlayerMenu()
        {
            UIManager.Instance.ShowGameMenu();
        }

        public void StopGame()
        {
            MultiplayerManager.Instance.LeaveGame();

            UIManager.Instance.ShowTitle();
        }

        public void Resume ()
        {
            InputManager.Instance.EnableMenuActions(false);
            InputManager.Instance.EnablePlayerActions(true);
        }

        public void Pause ()
        {
            InputManager.Instance.EnableMenuActions(true);
            InputManager.Instance.EnablePlayerActions(false);
        }

#if false
        /// <summary>
        /// Choose an island to spawn using the given level.  The home island is considered level 1.
        /// </summary>
        /// <param name="level">Level to spawn island at</param>
        /// <returns>Island</returns>
        public Island ChooseIsland (int level)
        {
            // TODO: need a way to figure out the connection points

        }
#endif



        private const int IslandGridSize = 64;
        private const int IslandGridCenter = IslandGridSize / 2;

        private Island[] _islandGrid = new Island[IslandGridSize * IslandGridSize];
        private List<IslandConnector> _islandConnectors = new List<IslandConnector>();
        private List<Island> _islands = new List<Island>();

        public Island GetIsland(int x, int y) =>_islandGrid[IslandGridCenter + x + (y + IslandGridCenter) * IslandGridSize];
        public void SetIsland (int x, int y, Island island) => _islandGrid[IslandGridCenter + x + (y + IslandGridCenter) * IslandGridSize] = island;
        public IslandConnection GetIslandConnections(int x, int y) => GetIsland(x, y)?.Connections ?? 0;

        private class IslandConnector
        {
            /// <summary>
            /// True if the connection has been made
            /// </summary>
            public bool IsConnected;

            /// <summary>
            /// Grid cell connecting from
            /// </summary>
            public Vector2Int From;

            /// <summary>
            /// Grid cell connecting to
            /// </summary>
            public Vector2Int To;
        }

        public List<Vector2Int> GetOpenConnectableCells ()
        {
            var cells = new List<Vector2Int>(); 
            foreach(var island in _islands)
            {
                var e = GetIsland(island.Cell.x + 1, island.Cell.y);
                if (e == null && (island.AvailableConnections & IslandConnection.East) != 0) cells.Add(new Vector2Int(island.Cell.x + 1, island.Cell.y));

                var w = GetIsland(island.Cell.x - 1, island.Cell.y);
                if (w == null && (island.AvailableConnections & IslandConnection.West) != 0) cells.Add(new Vector2Int(island.Cell.x - 1, island.Cell.y));

                var n = GetIsland(island.Cell.x, island.Cell.y + 1);
                if (n == null && (island.AvailableConnections & IslandConnection.North) != 0) cells.Add(new Vector2Int(island.Cell.x, island.Cell.y + 1));

                var s = GetIsland(island.Cell.x, island.Cell.y - 1);
                if (s == null && (island.AvailableConnections & IslandConnection.South) != 0) cells.Add(new Vector2Int(island.Cell.x, island.Cell.y - 1));
            }

            return cells;
        }

        public Island SpawnIsland (int x, int y, int level)
        {
            var require = GetRequiredConnections(x, y);
            var disallow = GetDisallowConnections(x, y);

            var islands = _biomes
                .Where(b => level >= b.MinLevel && level <= b.MaxLevel)
                .SelectMany(b => b.Islands)
                .Where(i => i.CheckRequirements(require, disallow))
                .ToArray();

            if (islands.Length == 0)
                return null;

            var island = islands[Random.Range(0,islands.Length)];
            var rotations = new List<int>(4);
            for(int i=0; i<4; i++) if (island.CheckRequirements(require, disallow, i)) rotations.Add(i);

            if (rotations.Count == 0)
                return null;

            var rotate = rotations[Random.Range(0,rotations.Count)];
            var prefab = island;

            island = Instantiate(island, new Vector3(x * 12, 0, y * 12), Quaternion.Euler(0,90 * rotate, 0));
            island.Cell = new Vector2Int(x, y);
            island.Biome = prefab.Biome;
            island.Connections = Island.Rotate(island.Connections, rotate);
            island.OpenConnections = require;
            island.AvailableConnections = (island.Connections & ~island.OpenConnections);
            SetIsland(x, y, island);
            _islands.Add(island);

            return island;
        }

        public IslandConnection Rotate (IslandConnection connection, int count)
        {
            count = count % 4;
            var c = (uint)connection;
            c = c << count;
            return (IslandConnection)((c & 0x0000000F) | ((c & 0xFFFFFFF0) >> 4));
        }

        /// <summary>
        /// Return the connections that would be required to place an island in the given spot
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <returns>Required connections</returns>
        private IslandConnection GetRequiredConnections(int x, int y) =>
            ((GetIslandConnections(x - 1, y) & IslandConnection.East) == 0 ? 0 : IslandConnection.West) |
            ((GetIslandConnections(x + 1, y) & IslandConnection.West) == 0 ? 0 : IslandConnection.East) |
            ((GetIslandConnections(x, y + 1) & IslandConnection.South) == 0 ? 0 : IslandConnection.North) |
            ((GetIslandConnections(x, y - 1) & IslandConnection.North) == 0 ? 0 : IslandConnection.South);

        private IslandConnection GetDisallowConnections(int x, int y)
        {
            var disallow = (IslandConnection)0;
            var west = GetIsland(x - 1, y);
            if (west != null && (west.Connections & IslandConnection.East) == 0)
                disallow |= IslandConnection.West;

            var east = GetIsland(x + 1, y);
            if (east != null && (east.Connections & IslandConnection.West) == 0)
                disallow |= IslandConnection.East;

            var north = GetIsland(x, y + 1);
            if (north != null && (north.Connections & IslandConnection.South) == 0)
                disallow |= IslandConnection.North;

            var south = GetIsland(x, y - 1);
            if (south != null && (south.Connections & IslandConnection.North) == 0)
                disallow |= IslandConnection.South;

            return disallow;
        }
    }
}

