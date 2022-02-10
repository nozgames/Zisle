using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// WARNING DO NOT REORDER THESE ATTRIBUTES!!!!
    /// </summary>
    public enum ActorAttribute
    {
        HealthMax,
        HealthRegen,
        Speed,
        Attack,
        AttackSpeed,
        Defense,
        Harvest,
        Build
    }

    /// <summary>
    /// Represents the current state of an Actor attribute
    /// </summary>
    public struct ActorAttributeValue
    {
        /// <summary>
        /// Current value
        /// </summary>
        public float Value;

        public float Add;

        public float Multiply;
    }
}