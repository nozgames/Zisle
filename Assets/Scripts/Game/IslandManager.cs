using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.AI.Navigation;
using Unity.Netcode;

namespace NoZ.Zisle
{

    /// <summary>
    /// 
    /// 
    /// 
    /// IDEAS:
    ///    - BIAS tiles that will create connections that are further from home rather than closer
    ///    - BRANCH bias that increases proportionally to the number of open open branches vs the number of tiles
    ///             - more likely to branch when there are no open branches
    ///  
    /// ISSUES:
    ///    - How to handle a loop when there are no branches?
    ///         - Lose ?
    ///         - Win ?
    ///         - Choose a random tile and swap it with one that has a branch, trying to maintain buildings ? 
    ///             - any buildings on a road have to get moved
    ///             
    /// 
    /// </summary>
    public class IslandManager : Singleton<IslandManager>
    {
        private const int IslandGridSize = 64;
        private const int IslandGridCenter = IslandGridSize / 2 + (IslandGridSize / 2) * IslandGridSize;
        private const int MaxCells = 32 * 32;

        [SerializeField] private Biome[] _biomes = null;
        [SerializeField] private NavMeshSurface _navMesh = null;

        private List<Cell> _cells = new List<Cell>();
        private Cell[] _cellGrid = new Cell[IslandGridSize * IslandGridSize];

        public void UpdateNavMesh()
        {
            _navMesh.BuildNavMesh();
        }

        public override void Initialize()
        {
            base.Initialize();

            foreach (var biome in _biomes)
                biome.RegisterNetworkId();

            Debug.Log("Bake Start");
            //_navMesh = transform.GetComponentInChildren<NavMeshSurface>();
            //_navMesh.BuildNavMesh();
            Debug.Log("Bake End");
        }

        public void ClearIslands()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
            {
                for(int i=transform.childCount-1; i>=0; i--)
                    DestroyImmediate(transform.GetChild(i).gameObject);
            }
            else
#endif
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                    Destroy(transform.GetChild(i).gameObject);
            }

            _cells.Clear();
            _cellGrid = new Cell[IslandGridSize * IslandGridSize];

            GetComponent<BoxCollider>().enabled = true;
            _navMesh.BuildNavMesh();
        }

        public void SpawnIslands ()
        {
            ClearIslands();

            GenerateCells();

            for (int i = 0; i < _cells.Count; i++)
            {
                var cell = _cells[i];
                if (cell.ConnectionMask == 0)
                    continue;

                var rotations = cell.Biome.Islands.SelectMany(i => i.GetRotations()).Where(r => r.Mask == cell.ConnectionMask).ToArray();
                if (rotations.Length == 0)
                    continue;

                var rotation = rotations[Random.Range(0, rotations.Length)];
                cell.Island = Instantiate(rotation.Island, new Vector3(cell.Position.x * 12.0f, 0, cell.Position.y * -12.0f), Quaternion.Euler(0, 90 * rotation.Rotation, 0), transform).GetComponent<Island>();
                cell.Island.GetComponent<MeshRenderer>().material = cell.Biome.Material;

                foreach (var netobj in cell.Island.GetComponentsInChildren<NetworkObject>())
                    netobj.Spawn();

                //cell.Island.gameObject.hideFlags = HideFlags.HideAndDontSave;

                if (cell.From != null)
                {
                    if (cell.Biome.Bridge != null)
                    {
                        var go = Instantiate(cell.Biome.Bridge, (cell.From.Island.transform.position + cell.Island.transform.position) * 0.5f, Quaternion.identity, transform);

                        //if(GameManager.Instance.Options.SpawnEnemies)
                            //go.AddComponent<EnemySpawner>();
                    }
                }
            }

            GetComponent<BoxCollider>().enabled = false;
            _navMesh.BuildNavMesh();
        }


        public void GenerateIslands()
        {
#if false
            ClearIslands();
    
            GenerateCells(_defaultOptions);

            for (int i = 0; i < _cells.Count; i++)
            {
                var cell = _cells[i];
                if (cell.ConnectionMask == 0)
                    continue;

                var rotations = cell.Biome.Islands.SelectMany(i => i.GetRotations()).Where(r => r.Mask == cell.ConnectionMask).ToArray();
                if (rotations.Length == 0)
                    continue;

                var rotation = rotations[Random.Range(0, rotations.Length)];
                cell.Island = Instantiate(rotation.Island, new Vector3(cell.Position.x * 12.0f, 0, cell.Position.y * -12.0f), Quaternion.Euler(0, 90 * rotation.Rotation, 0), transform).GetComponent<Island>();
                cell.Island.GetComponent<MeshRenderer>().material = cell.Biome.Material;
                //cell.Island.gameObject.hideFlags = HideFlags.HideAndDontSave;

                if(cell.From != null)
                {
                    if(cell.Biome.Bridge != null)
                    {
                        Instantiate(cell.Biome.Bridge, (cell.From.Island.transform.position + cell.Island.transform.position) * 0.5f, Quaternion.identity, transform);
                    }
                }
            }
#endif
        }

        public class Cell
        {
            /// <summary>
            /// Position of the cell
            /// </summary>
            public Vector2Int Position;

            /// <summary>
            /// Level of the island, the further away from home the higher the level
            /// </summary>
            public int Level;

            /// <summary>
            /// Connection mask used to determine which islands can be placed in this cell
            /// </summary>
            public uint ConnectionMask; 

            /// <summary>
            /// Biome of the island that should be spawned in this cell
            /// </summary>
            public Biome Biome;

            /// <summary>
            /// Island that was spawned for this cell, will be null until the island is opened
            /// </summary>
            public Island Island;

            /// <summary>
            /// Cell the path came from
            /// </summary>
            public Cell From;
        }

        /// <summary>
        /// Returns true if the given position is a valid cell in the cell grid
        /// </summary>
        private bool IsValidCell(Vector2Int position) =>
            GetCellIndex(position) >= 0 && GetCellIndex(position) < _cellGrid.Length;

        /// <summary>
        /// Return the index of the cell at the given position within the cell grid
        /// </summary>
        private int GetCellIndex(Vector2Int position) =>
            IslandGridCenter + position.x + position.y * IslandGridSize;
        
        /// <summary>
        /// Return the cell at the given position
        /// </summary>
        private Cell GetCell(Vector2Int position) => _cellGrid[GetCellIndex(position)];

        /// <summary>
        /// Add a new cell at the given position 
        /// </summary>
        private Cell AddCell (Vector2Int position, Cell from, Biome biome)
        {
            Assert.IsNull(GetCell(position));

            var cell = new Cell { Biome = biome, Position = position };
            _cells.Add(cell);
            _cellGrid[GetCellIndex(position)] = cell;

            // Add connection masks if coming from another cell
            if(from != null)
            {
                cell.Level = from.Level + 1;
                cell.ConnectionMask |= (from.Position - position).ToDirection().ToMask();
                from.ConnectionMask |= (position - from.Position).ToDirection().ToMask();
            }

            cell.From = from;

            return cell;
        }

        /// <summary>
        /// Generate all of the cells for the game and return the home cell
        /// </summary>
        private Cell GenerateCells ()
        {
            var options = GameManager.Instance.Options;

#if UNITY_EDITOR
            Random.InitState((int)(UnityEditor.EditorApplication.timeSinceStartup * 1000.0));
#else
            Random.InitState((int)(Time.realtimeSinceStartupAsDouble * 1000.0));
#endif

            var forks = new List<Vector2Int>(4);
            var queue = new Queue<Cell>();

            var homeBiomes = _biomes.Where(b => 0 >= b.MinLevel && 0 <= b.MaxLevel).ToArray();
            var homeBiome= homeBiomes[Random.Range(0, homeBiomes.Length)];

            // Queue the home cell
            var home = AddCell(Vector2Int.zero, null, homeBiome);
            queue.Enqueue(home);

            // Fill all cells until we are done
            while (_cells.Count < options.MaxIslands)
            {
                while (queue.Count > 0 && _cells.Count < options.MaxIslands)
                {
                    // Add the island to the list of islands
                    var cell = queue.Dequeue();

                    // Find all possible forks that can be taken
                    // If no forks are availble then an end cap will be placed
                    GetAvailableForks(cell, forks);
                    if (forks.Count == 0)
                        continue;

                    // Choose a random number of forks
                    var forkCount = 0;
                    if (cell.Position == Vector2Int.zero)
                        forkCount = GameManager.Instance.Options.StartingLanes;
                    else
                        forkCount = RandomWeightedIndex(options.GetForkWeights(), 0, forks.Count + 1, (f) => f);

                    // Do not allow all paths to close by making sure there is at least one 
                    // other cell to expand from
                    if (queue.Count == 0)
                        forkCount = Mathf.Max(forkCount, 1);

                    for (int i = 0; i < forkCount; i++)
                    {
                        var forkIndex = Random.Range(0, forks.Count);
                        var fork = forks[forkIndex];
                        forks.RemoveAt(forkIndex);

                        // Find a biome
                        var level = cell.Level + 1;
                        var biomes = _biomes.Where(b => level >= b.MinLevel && level <= b.MaxLevel).ToArray();
                        var biome = biomes[Random.Range(0, biomes.Length)];

                        // Add a new cell for the fork
                        queue.Enqueue(AddCell(fork, cell, biome));
                    }
                }

                // Hit a dead end, need to force a fork somewhere
                if (_cells.Count < options.MaxIslands)
                {
                    // Walk backwards from the end and pick the first cell we can find to branch from
                    var stop = true;
                    for(int i=_cells.Count-1; i>0 && stop; i--)
                    {
                        var cell = _cells[i];
                        GetAvailableForks(cell, forks);
                        if (forks.Count == 0)
                            continue;

                        AddCell(forks[Random.Range(0, forks.Count)], cell, cell.Biome);
                        stop = false;
                    }

                    if (stop)
                        break;
                }
            }

            return home;
        }

        private static List<float> _weights = new List<float>(1024);

        private int GetAvailableForks (Cell cell, List<Vector2Int> forks)
        {
            forks.Clear();
            for (int i = 0; i < 4; i++)
            {
                // Skip our incoming paths
                var dir = (CardinalDirection)i;
                if ((cell.ConnectionMask & dir.ToMask()) != 0)
                    continue;

                // Skip this path if it is off the grid
                var position = cell.Position + dir.ToOffset();
                if (!IsValidCell(position))
                    continue;

                // If there is already a cell at the this position then we cannot fork that direction
                if (null != GetCell(position))
                    continue;

                forks.Add(position);
            }

            return forks.Count;
        }

        private int RandomWeightedIndex<T> (IEnumerable<T> items, int start, int count, System.Func<T,float> getWeight)
        {
            var totalWeight = 0.0f;
            _weights.Clear();
            foreach(var item in items)
            {
                if(start > 0)
                {
                    start--;
                    continue;
                }

                var weight = Mathf.Max(getWeight(item),0.0f);
                _weights.Add(weight);
                totalWeight += weight;

                if (--count == 0)
                    break;
            }

            if (_weights.Count == 0)
                return 0;

            // Chooose a number between 0 and totalWeight and find the item that falls in that range
            var random = Random.Range(0.0f, totalWeight);
            var choice = 0;
            for (; choice < _weights.Count - 1 && random > _weights[choice] - float.Epsilon; choice++)
                random -= _weights[choice];

            return choice;
        }
    }
}
