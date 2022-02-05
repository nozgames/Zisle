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
        public static Vector2Int ToOffset (this CardinalDirection dir) => dir switch
        {
            CardinalDirection.North => new Vector2Int(0, -1),
            CardinalDirection.South => new Vector2Int(0, 1),
            CardinalDirection.East => new Vector2Int(1, 0),
            CardinalDirection.West => new Vector2Int(-1, 0),
            _ => throw new System.NotSupportedException()
        };

        public static uint ToMask(this CardinalDirection dir) =>
            (uint)(1 << (int)dir);


        public static CardinalDirection ToDirection (this Vector2Int cell)
        {
            if (cell.x == -1 && cell.y == 0) return CardinalDirection.West;
            if (cell.x == 1 && cell.y == 0) return CardinalDirection.East;
            if (cell.x == 0 && cell.y == -1) return CardinalDirection.North;
            if (cell.x == 0 && cell.y == 1) return CardinalDirection.South;
            throw new System.NotSupportedException(cell.ToString());
        }
    }
}
