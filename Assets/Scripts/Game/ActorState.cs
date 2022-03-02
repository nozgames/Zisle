using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Current high level state of the actor
    /// </summary>
    public enum ActorState
    {
        /// <summary>
        /// Actor has no current state
        /// </summary>
        None,

        /// <summary>
        /// Actor is spawning
        /// </summary>
        Spawn,

        /// <summary>
        /// Actor is playing an intro sequence
        /// </summary>
        Intro,

        /// <summary>
        /// Actor is active and thinking
        /// </summary>
        Active,

        /// <summary>
        /// Actor is dead
        /// </summary>
        Dead
    }
}
