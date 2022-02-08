using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Represents a single cell in the island grid
    /// </summary>
    public struct IslandCell : INetworkSerializable
    {
        /// <summary>
        /// Position of the cell
        /// </summary>
        public Vector2Int Position;

        /// <summary>
        /// Position of the cell the path of this cell moves towards
        /// </summary>
        public Vector2Int To;

        /// <summary>
        /// Level of the island (distance from home)
        /// </summary>
        public int Level;

        /// <summary>
        /// Identifier of the biome used for this island
        /// </summary>
        public ushort BiomeId;

        /// <summary>
        /// Index of the island within the biome
        /// </summary>
        public int IslandIndex;

        /// <summary>
        /// Rotation of the island
        /// </summary>
        public int Rotation;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsWriter)
            {
                var writer = serializer.GetFastBufferWriter();
                writer.TryBeginWrite(5);
                writer.WriteValue((byte)IslandIndex);
                writer.WriteValue((byte)Level);
                using (var bitWriter = writer.EnterBitwiseContext())
                {
                    bitWriter.WriteBits((byte)BiomeId, 5);
                    bitWriter.WriteBits((byte)Position.x, 4);
                    bitWriter.WriteBits((byte)Position.y, 4);
                    bitWriter.WriteBits((byte)To.x, 4);
                    bitWriter.WriteBits((byte)To.y, 4);
                    bitWriter.WriteBits((byte)Rotation, 2);
                }
            }
            else
            {
                var reader = serializer.GetFastBufferReader();
                reader.TryBeginRead(5);
                byte b;
                reader.ReadValue(out b);
                IslandIndex = b;
                reader.ReadValue(out b);
                Level = b;
                using (var bitreader = reader.EnterBitwiseContext())
                {
                    // Biome
                    ulong x;
                    ulong y;
                    bitreader.ReadBits(out x, 5);
                    BiomeId = (ushort)x;

                    // Position
                    bitreader.ReadBits(out x, 4);
                    bitreader.ReadBits(out y, 4);
                    Position = new Vector2Int((int)x, (int)y);

                    // To
                    bitreader.ReadBits(out x, 4);
                    bitreader.ReadBits(out y, 4);
                    To = new Vector2Int((int)x, (int)y);

                    // Rotation
                    bitreader.ReadBits(out b, 2);
                    Rotation = b;
                }
            }
        }
    }
}
