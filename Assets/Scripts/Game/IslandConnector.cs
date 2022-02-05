using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public struct IslandConnector
    {
        /// <summary>
        /// Connected island
        /// </summary>
        public Island Island;

        /// <summary>
        /// Cardinal direction of the connection relative to the island
        /// </summary>
        public CardinalDirection Direction;
    }
}
