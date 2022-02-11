using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Defines the grid of islands within the world
    /// </summary>
    public static class IslandGrid
    {
        public const int Size = 16;
        public const int Min = 0;
        public const int Max = Size - 1;
        public const int Center = Size / 2;
        public const int IndexMax = Size * Size;
        public const int IndexCenter = Center + Center * Size;

        public static readonly Vector2Int CenterCell = new Vector2Int(Center, Center);

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
            new Vector3((cell.x - Center) * (IslandMesh.GridSize + 1), 0, -((cell.y - Center) * (IslandMesh.GridSize + 1)));

        /// <summary>
        /// Converts the given grid array index to a world coordinate
        /// </summary>
        public static Vector3 IndexToWorld(int index) =>
            CellToWorld(IndexToCell(index));

        /// <summary>
        /// Converts the given world coordinate to a grid cell
        /// </summary>
        public static Vector2Int WorldToCell(Vector3 world) =>
            new Vector2Int(
                Mathf.RoundToInt((world.x / (IslandMesh.GridSize + 1)) + Center),
                Mathf.RoundToInt((-world.z / (IslandMesh.GridSize + 1)) + Center));

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
    }
}
