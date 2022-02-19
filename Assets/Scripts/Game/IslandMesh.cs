using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public enum IslandTile
    {
        Water,
        Path,
        Grass,
        Grass2,
        Grass3,

        None = 255
    }

    public enum IslandType
    {
        Default,
        Boss
    }

    /// <summary>
    /// Manages all island related spawning
    /// </summary>
    //[ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class IslandMesh : NetworkBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField] private IslandType _type = IslandType.Default;
        [SerializeField] private float _weight = 1.0f;

        [HideInInspector]
        [SerializeField] private IslandTile[] _tiles = null;

        public const int GridSize = 11;
        public const int GridMin = 0;
        public const int GridCenter = GridSize / 2;
        public const int GridMax = GridSize - 1;
        public const int GridIndexMax = GridSize * GridSize;

        /// <summary>
        /// Position of the island within the island grid
        /// </summary>
        public Vector2Int Position { get; set; }

        /// <summary>
        /// Biome the island is from
        /// </summary>
        public Biome Biome { get; set; }

        /// <summary>
        /// Return the island type
        /// </summary>
        public IslandType Type => _type;

        /// <summary>
        /// Weight of the island when choosing randomly from within the parent biome
        /// </summary>
        public float Weight => _weight;

        /// <summary>
        /// Island tiles
        /// </summary>
        public IslandTile[] Tiles { get => _tiles; set => _tiles = value; }

        /// <summary>
        /// Returns the mask that represents the available connections using cardinal directions 
        /// </summary>
        public CardinalDirectionMask ConnectionMask { get; private set; }

        /// <summary>
        /// Number of connections this island has
        /// </summary>
        public int ConnectionCount { get; private set; }

        /// <summary>
        /// Returns true if the island has a connection in the given cardinal direction
        /// </summary>
        /// <param name="dir">Cardinal direction</param>
        /// <returns>True if there is a valid connection the given cardinal direction</returns>
        public bool HasConnection (CardinalDirection dir) => (dir.ToMask() & ConnectionMask) != 0;

        /// <summary>
        /// Return the tile at the given position
        /// </summary>
        public IslandTile GetTile(Vector2Int cell)
        {
            if (!IsValidCell(cell))
                return IslandTile.None;

            return _tiles[CellToIndex(cell)];
        }

        /// <summary>
        /// Return the tile at the given grid index
        /// </summary>
        public IslandTile GetTile(int index) => GetTile(index, Vector2Int.zero);

        /// <summary>
        /// Return the tile at <paramref name="index"/> offset by <paramref name="offset"/>
        /// </summary>
        public IslandTile GetTile(int index, Vector2Int offset) => GetTile(IndexToCell(index) + offset);

        /// <summary>
        /// Returns true if the given tile matches the actual tile at the given position
        /// </summary>
        public bool IsTile(Vector2Int position, IslandTile tile) => GetTile(position) == tile;

        /// <summary>
        /// Returns true if the tile at the given index is the given tile
        /// </summary>
        public bool IsTile(int index, IslandTile tile) => GetTile(index) == tile;

        /// <summary>
        /// Set the tile at the given position
        /// </summary>
        public void SetTile(Vector2Int cell, IslandTile tile) => _tiles[CellToIndex(cell)] = tile;

        /// <summary>
        /// Returns true if the given index is within the Island Mesh
        /// </summary>
        public bool IsValidIndex(int index) => index >= 0 && index < GridIndexMax;

        /// <summary>
        /// Returns true if the given cell is a valid coordinate within the mesh
        /// </summary>
        public bool IsValidCell(Vector2Int cell) =>
            cell.x >= GridMin && cell.y >= GridMin && cell.x <= GridMax && cell.y <= GridMax;

        /// <summary>
        /// Returns true if the tile at <paramref name="index"/> with the <paramref name="offset"/> matches the <paramref name="tile"/>
        /// </summary>
        public bool IsTile(int index, Vector2Int offset, IslandTile tile) =>
            GetTile(index, offset) == tile;

        /// <summary>
        /// Convert a grid cell to a grid array index
        /// </summary>
        public static int CellToIndex (Vector2Int cell) => cell.x + cell.y * GridSize;

        /// <summary>
        /// Convert a grid array index to a cell
        /// </summary>
        public static Vector2Int IndexToCell(int index) => new Vector2Int(index % GridSize, index / GridSize);

        /// <summary>
        /// Converts the given cell coordinate to a world coordinate
        /// </summary>
        public static Vector3 CellToLocal(Vector2Int cell) =>
            new Vector3(cell.x - GridCenter, 0, -(cell.y - GridCenter));

        /// <summary>
        /// Convert a grid array index to a world coordinate
        /// </summary>
        public static Vector3 IndexToLocal(int index) => CellToLocal(IndexToCell(index));

        /// <summary>
        /// Convert a local island coordinate to a cell
        /// </summary>
        public static Vector2Int LocalToCell (Vector3 local) =>
            new Vector2Int(Mathf.RoundToInt(local.x + GridCenter), Mathf.RoundToInt(GridCenter - local.z));


        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (_tiles == null)
                return;

            var west = IsTile(new Vector2Int(GridMin, GridCenter), IslandTile.Path);
            var east = IsTile(new Vector2Int(GridMax, GridCenter), IslandTile.Path);
            var north = IsTile(new Vector2Int(GridCenter, GridMin), IslandTile.Path);
            var south = IsTile(new Vector2Int(GridCenter, GridMax), IslandTile.Path);

            ConnectionMask =
                (west ? CardinalDirection.West.ToMask() : 0) |
                (east ? CardinalDirection.East.ToMask() : 0) |
                (north ? CardinalDirection.North.ToMask() : 0) |
                (south ? CardinalDirection.South.ToMask() : 0);
            
            ConnectionCount = (west ? 1 : 0) + (east ? 1 : 0) + (north ? 1 : 0) + (south ? 1 : 0);
        }

        public struct IslandRotation
        {
            public IslandMesh Island;
            public CardinalDirectionMask Mask;
            public CardinalDirection Rotation;
        }

        public IEnumerable<IslandRotation> GetRotations ()
        {
            for(int i=0; i<4; i++)
            {
                yield return new IslandRotation
                {
                    Island = this,
                    Mask = ConnectionMask.Rotate(i),
                    Rotation = (CardinalDirection)i
                };
            }
        }
    }
}
