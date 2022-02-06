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

    public struct ActorAttributeValue
    {
        public float Value;
        public float Add;
        public float Multiply;
    }
}