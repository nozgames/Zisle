using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
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
    public class WorldGenerator
    {
        private List<Cell> _cells;
        private Cell[] _cellGrid;

        [System.Serializable]
        public struct Options
        {
            public int MaxIslands;
            public float PathWeight0;
            public float PathWeight1;
            public float PathWeight2;
            public float PathWeight3;

            public int StartingLanes;

            public IEnumerable<float> GetForkWeights()
            {
                yield return PathWeight0;
                yield return PathWeight1;
                yield return PathWeight2;
                yield return PathWeight3;
            }
        }

        private class Cell
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
            public CardinalDirectionMask ConnectionMask;

            /// <summary>
            /// Biome of the island that should be spawned in this cell
            /// </summary>
            public Biome Biome;

            /// <summary>
            /// Island that was spawned for this cell, will be null until the island is opened
            /// </summary>
            public IslandMesh Island;

            /// <summary>
            /// Cell the path came from
            /// </summary>
            public Cell From;

            public CardinalDirection Rotation;
        }

        /// <summary>
        /// Generate an array of island cells using the given options
        /// </summary>
        public IslandCell[] Generate (Options options)
        {
            _cells = new List<Cell>();
            _cellGrid = new Cell[IslandGrid.IndexMax];

            GenerateCells(options);

            // Choose random islands for all cells
            foreach(var c in _cells)
            {
                var rotations = c.Biome.Islands
                    .Where(i => i.Type == IslandType.Default)
                    .SelectMany(i => i.GetRotations())
                    .Where(r => r.Mask == c.ConnectionMask)
                    .ToArray();

                // Choose a random island
                var rot = rotations[Random.Range(0, rotations.Length)];
                c.Island = rot.Island;
                c.Rotation = rot.Rotation;
            }

            // Find the cell to place to boss island
            var bossCells = _cells.Where(c => c.Island.ConnectionCount == 1).ToArray();
            var bossCellIndex = WeightedRandom.RandomWeightedIndex(bossCells, 0, bossCells.Length, (c) => c.Level);
            var bossCell = bossCells[WeightedRandom.RandomWeightedIndex(bossCells, 0, bossCells.Length, (c) => c.Level)];

            // Now replace the island in the boss cell with a boss island from the same biome
            var bossIslands = bossCell.Biome.Islands.Where(i => i.Type == IslandType.Boss && i.ConnectionCount == 1).ToArray();
            var bossIsland = bossIslands[WeightedRandom.RandomWeightedIndex(bossIslands, 0, bossIslands.Length, (i) => i.Weight)];
            bossCell.Island = bossIsland;

            // Generate results
            var result = _cells.Where(c => c.ConnectionMask != 0).Select(c =>
            {
                if (c.Island == null)
                    return new IslandCell { IslandIndex = -1 };

                // Return the generated island cell
                return new IslandCell
                {
                    Position = c.Position,
                    To = c.From?.Position ?? IslandGrid.CenterCell,
                    Level = c.Level,
                    BiomeId = c.Biome.NetworkId,
                    IslandIndex = c.Biome.IndexOf(c.Island),
                    Rotation = c.Rotation
                };
            }).Where(nc => nc.IslandIndex != -1).ToArray();          

            _cells.Clear();
            _cells = null;

            _cellGrid = null;

            return result;
        }
       
        /// <summary>
        /// Return the cell at the given position
        /// </summary>
        private Cell GetCell(Vector2Int position) => _cellGrid[IslandGrid.CellToIndex(position)];

        /// <summary>
        /// Add a new cell at the given position 
        /// </summary>
        private Cell AddCell (Vector2Int position, Cell from, Biome biome)
        {
            Assert.IsNull(GetCell(position));

            var cell = new Cell { Biome = biome, Position = position };
            _cells.Add(cell);
            _cellGrid[IslandGrid.CellToIndex(position)] = cell;

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
        private void GenerateCells (Options options)
        {
#if UNITY_EDITOR
            Random.InitState((int)(UnityEditor.EditorApplication.timeSinceStartup * 1000.0));
#else
            Random.InitState((int)(Time.realtimeSinceStartupAsDouble * 1000.0));
#endif

            var forks = new List<Vector2Int>(4);
            var queue = new Queue<Cell>();

            var homeBiomes = GameManager.Instance.Biomes.Where(b => 0 >= b.MinLevel && 0 <= b.MaxLevel).ToArray();
            var homeBiome = homeBiomes[Random.Range(0, homeBiomes.Length)];

            // Queue the home cell
            var home = AddCell(IslandGrid.CenterCell, null, homeBiome);
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
                    if (cell.Position == IslandGrid.CenterCell)
                        forkCount = options.StartingLanes;
                    else
                        forkCount = WeightedRandom.RandomWeightedIndex(options.GetForkWeights(), 0, forks.Count + 1, (f) => f);

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
                        var biomes = GameManager.Instance.Biomes.Where(b => level >= b.MinLevel && level <= b.MaxLevel).ToArray();
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
        }

        /// <summary>
        /// Determine which forks are available from the given cell
        /// </summary>
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
                if (!IslandGrid.IsValidCell(position))
                    continue;

                // If there is already a cell at the this position then we cannot fork that direction
                if (null != GetCell(position))
                    continue;

                forks.Add(position);
            }

            return forks.Count;
        }
    }
}
