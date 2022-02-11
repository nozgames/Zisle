using UnityEngine;

namespace NoZ.Zisle
{
    public enum CardinalDirection
    {
        North,          // -Y
        East,           // +X
        South,          // +Y
        West            // -X
    }

    public static class CardinalDirectionHelpers
    {
        private static readonly Vector2Int[] OffsetTable = new Vector2Int[]
        {
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0)
        };

        private static readonly Vector3[] WorldTable = new Vector3[]
        {
            new Vector3(0,0,1),
            new Vector3(1,0,0),
            new Vector3(0,0,-1),
            new Vector3(-1,0,0)
        };

        /// <summary>
        /// Rotate the cardinal direction the given number of times clockwise
        /// </summary>
        public static CardinalDirection Rotate (this CardinalDirection dir, int count) =>
            (CardinalDirection)((((int)dir) + count) % 4);

        /// <summary>
        /// Return the opposite direction 
        /// </summary>
        public static CardinalDirection Opposite(this CardinalDirection dir) =>
            (CardinalDirection)((((int)dir) + 2) % 4);

        /// <summary>
        /// Returns the postion offset for the given cardinal direction
        /// </summary>
        public static Vector2Int ToOffset (this CardinalDirection dir) => OffsetTable[(int)dir];

        public static Vector3 ToWorld(this CardinalDirection dir) => WorldTable[(int)dir];

        public static uint ToMask(this CardinalDirection dir) =>
            (uint)(1 << (int)dir);

        /// <summary>
        /// Convert an offset to a direction
        /// </summary>
        public static CardinalDirection ToDirection (this Vector2Int offset)
        {
            for(int i=0; i<4; i++)
                if(offset == OffsetTable[i]) return (CardinalDirection)i;

            throw new System.NotSupportedException(offset.ToString());
        }
    }
}
