using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public enum IslandTile
    {
        Water,
        Grass,
        Path,

        None = 255
    }

    /// <summary>
    /// Manages all island related spawning
    /// </summary>
    //[ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class IslandMesh : NetworkBehaviour, ISerializationCallbackReceiver
    {
        public const int GridSize = 13;
        public const int GridCenter = 6;
        public const int GridIndexMax = GridSize * GridSize;


        [SerializeField] private IslandTile[] _tiles = null;

        /// <summary>
        /// Position of the island within the island grid
        /// </summary>
        public Vector2Int Position { get; set; }

        /// <summary>
        /// Biome the island is from
        /// </summary>
        public Biome Biome { get; set; }

        /// <summary>
        /// Island tiles
        /// </summary>
        public IslandTile[] Tiles { get => _tiles; set => _tiles = value; }

        /// <summary>
        /// Returns the mask that represents the available connections using cardinal directions 
        /// </summary>
        public uint ConnectionMask { get; private set; }

        /// <summary>
        /// Return the index of the tile at the given position within the tile array
        /// </summary>
        private int GetTileIndex(Vector2Int position) => position.x + position.y * 13;

        /// <summary>
        /// Return the tile position of the given tile index
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Vector2Int IndexToPosition (int index) => new Vector2Int (index % GridSize, index / GridSize);

        /// <summary>
        /// Converts a tile index to a world coordinate relative to the island
        /// </summary>
        public Vector3 IndexToWorld(int index)
        {
            var v = (IndexToPosition(index) - new Vector2Int(GridCenter, GridCenter)).ToVector3XZ();
            v.z *= -1;
            return v;
        }
        

        /// <summary>
        /// Return the tile at the given position
        /// </summary>
        public IslandTile GetTile(Vector2Int position) => _tiles[GetTileIndex(position)];

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
        public void SetTile(Vector2Int position, IslandTile tile) => _tiles[GetTileIndex(position)] = tile;

        public IslandTile GetTile(int index) => _tiles[index];

        public IslandTile GetTile (int index, Vector2Int offset)
        {
            index += offset.x;
            index += offset.y * GridSize;
            if (!IsValidIndex(index))
                return IslandTile.None;

            return _tiles[index];
        }

        public bool IsValidIndex(int index) => index >= 0 && index < GridIndexMax;

        public bool IsTile(int index, Vector2Int offset, IslandTile tile) =>
            GetTile(index, offset) == tile;

        public static uint RotateMask (uint mask, int count)
        {
            mask = mask << (count % 4);
            return (mask & 0x0000000F) | ((mask & 0xFFFFFFF0) >> 4);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            ConnectionMask =
                (IsTile(new Vector2Int(1, 6), IslandTile.Path) ? CardinalDirection.West.ToMask() : 0) |
                (IsTile(new Vector2Int(11, 6), IslandTile.Path) ? CardinalDirection.East.ToMask() : 0) |
                (IsTile(new Vector2Int(6, 1), IslandTile.Path) ? CardinalDirection.North.ToMask() : 0) |
                (IsTile(new Vector2Int(6, 11), IslandTile.Path) ? CardinalDirection.South.ToMask() : 0);
        }

        public struct IslandRotation
        {
            public IslandMesh Island;
            public uint Mask;
            public int Rotation;
        }

        public IEnumerable<IslandRotation> GetRotations ()
        {
            for(int i=0; i<4; i++)
            {
                yield return new IslandRotation
                {
                    Island = this,
                    Mask = RotateMask(ConnectionMask, i),
                    Rotation = i
                };
            }
        }
    }
}
