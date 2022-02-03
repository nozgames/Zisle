using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    [System.Flags]
    public enum IslandConnection
    {
        North = 1<<0,
        East = 1 << 1,
        South = 1<<2,
        West = 1<<3
    }

    /// <summary>
    /// Manages all island related spawning
    /// </summary>
    public class Island : NetworkBehaviour
    {
        [SerializeField] private IslandConnection _connections = 0;

        /// <summary>
        /// Grid cell the island is in
        /// </summary>
        public Vector2Int Cell { get; set; }

        /// <summary>
        /// Biome the island is from
        /// </summary>
        public Biome Biome { get; set; }

        public IslandConnection Connections
        {
            get => _connections;
            set => _connections = value;
        }

        /// <summary>
        /// Connection directions that have been used to connect to another island
        /// </summary>
        public IslandConnection OpenConnections { get; set; }

        public IslandConnection AvailableConnections { get; set; }

        /// <summary>
        /// Paths that are closed because they have not been connected to another island
        /// </summary>
        //public IslandConnection ClosedConnections { get; private set; }

        /// <summary>
        /// Check to see if the island meets the requirements for being placed in a cell
        /// </summary>
        /// <param name="require">Required connections</param>
        /// <returns>True if the island meets requirements</returns>
        public bool CheckRequirements(IslandConnection require, IslandConnection dissallow) =>
            CheckRequirements(require, dissallow, 0) ||
            CheckRequirements(require, dissallow, 1) ||
            CheckRequirements(require, dissallow, 2) ||
            CheckRequirements(require, dissallow, 3);

        public bool CheckRequirements(IslandConnection require, IslandConnection dissallow, int rotate)
        {
            var rotated = Rotate(Connections, rotate);
            return (rotated & require) == require && (rotated & dissallow) == 0;
        }

        public static IslandConnection Rotate (IslandConnection connection, int count)
        {
            count = count % 4;
            var c = (uint)connection;
            c = c << count;
            return (IslandConnection)((c & 0x0000000F) | ((c & 0xFFFFFFF0) >> 4));
        }

    }
}
