using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Returns a weight for spawning the given actor based on the current game state
    /// </summary>
    public abstract class SpawnWeight : ScriptableObject
    {
        public abstract float GetWeight (ActorDefinition actorDefinition);
    }
}
