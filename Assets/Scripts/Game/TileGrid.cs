using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Defines the grid of tiles within the world.  The world is made up of islands, each island
    /// has a number of tiles, this grid encompasses all tiles of all islands.
    /// </summary>
    public static class TileGrid
    {
        public const int Size = IslandMesh.GridSize * IslandGrid.Size + (IslandGrid.Size - 1);
        public const int Min = 0;
        public const int Max = Size - 1;
        public const int Center = Size / 2;
        public const int IndexMax = Size * Size;
        public const int IndexCenter = Center + Center * Size;

        /// <summary>
        /// Returns true if the given cell is within the bounds of the grid
        /// </summary>
        public static bool IsValidCell(Vector2Int cell) =>
            cell.x >= Min && cell.x <= Max && cell.y >= Min && cell.y <= Max;

        /// <summary>
        /// Returns true if the given grid array index is within the bounds of the grid
        /// </summary>
        public static bool IsValidIndex(int index) =>
            IsValidCell(IndexToCell(index));

        /// <summary>
        /// Convert an index in a grid array to a cell coordinate 
        /// </summary>
        public static Vector2Int IndexToCell(int index) =>
            new Vector2Int(index % Size, index / Size);

        /// <summary>
        /// Convert a cell coordinate to an index
        /// </summary>
        public static int CellToIndex(Vector2Int cell) =>
            cell.x + cell.y * Size;

        /// <summary>
        /// Converts the given cell coordinate to a world coordinate
        /// </summary>
        public static Vector3 CellToWorld(Vector2Int cell) =>
            new Vector3(cell.x - Center, 0, -(cell.y - Center));

        /// <summary>
        /// Converts the given grid array index to a world coordinate
        /// </summary>
        public static Vector3 IndexToWorld(int index) =>
            CellToWorld(IndexToCell(index));

            /// <summary>
        /// Converts the given world coordinate to a grid cell
        /// </summary>
        public static Vector2Int WorldToCell(Vector3 world) =>
            new Vector2Int((int)(world.x + 0.5f) + Center, (int)(-world.z + 0.5f) + Center);

        /// <summary>
        /// Converts the given world coordintate to a grid array index
        /// </summary>
        public static int WorldToIndex(Vector3 world) =>
            CellToIndex(WorldToCell(world));

        /// <summary>
        /// Offset the given cell using a cardinal direction
        /// </summary>
        public static Vector2Int OffsetCell(Vector2Int cell, CardinalDirection direction) =>
            cell + direction.ToOffset();

        /// <summary>
        /// Add the <paramref name="offset"/> to the <paramref name="cell"/>
        /// </summary>
        public static Vector2Int OffsetCell(Vector2Int cell, Vector2Int offset) =>
            cell + offset;

        /// <summary>
        /// Offset the given grid array index by the cardinal direction
        /// </summary>
        public static int OffsetIndex(int index, CardinalDirection direction) =>
            CellToIndex(IndexToCell(index) + direction.ToOffset());

        /// <summary>
        /// Add the given offset to the <paramref name="index"/>
        /// </summary>
        public static int OffsetIndex(int index, Vector2Int offset) =>
            CellToIndex(IndexToCell(index) + offset);

        /// <summary>
        /// Converts a tile cell into an island cell.  Note the gap cells are considered part 
        /// of the island cell that proceeds them.
        /// </summary>
        public static Vector2Int CellToIslandCell(Vector2Int cell) =>
            new Vector2Int(cell.x / (IslandMesh.GridSize + 1), cell.y / (IslandMesh.GridSize + 1));

        /// <summary>
        /// Converts an island cell to a tile cell.  Note that the tile cell will be the cell in
        /// the center of the island.
        /// </summary>
        public static Vector2Int IslandCellToCell(Vector2Int islandCell) =>
            new Vector2Int(islandCell.x * (IslandMesh.GridSize + 1) + IslandMesh.GridCenter, islandCell.y * (IslandMesh.GridSize + 1) + IslandMesh.GridCenter);

        /// <summary>
        /// Converts a tile grid array index to an island grid array index
        /// </summary>
        public static int IndexToIslandIndex(int index) =>
            IslandGrid.CellToIndex(CellToIslandCell(IndexToCell(index)));

        /// <summary>
        /// Converts a island grid array index to an tile grid array index
        /// </summary>
        /// <param name="islandIndex"></param>
        /// <returns></returns>
        public static int IslandIndexToIndex(int islandIndex) =>
            CellToIndex(IslandCellToCell(IslandGrid.IndexToCell(islandIndex)));
    }
}
