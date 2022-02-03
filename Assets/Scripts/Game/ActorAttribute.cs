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

    [System.Serializable]
    public class ActorAttributeValue
    {
        public float BaseValue;

        [System.NonSerialized] public float CurrentValue;
        [System.NonSerialized] public float Add;
        [System.NonSerialized] public float Multiply;
    }

}