using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Manages a running game
    /// </summary>
    public class Game : NetworkBehaviour
    {
        // TODO: spawn islands
        // TODO: manage the players connected 

        /// <summary>
        /// List of connected players
        /// </summary>
        private List<PlayerController> _players = new List<PlayerController>();



    }
}
