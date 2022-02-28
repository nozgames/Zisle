using UnityEngine;

namespace NoZ.Zisle
{
    public abstract class AbilityCondition : ScriptableObject
    {
        public abstract float CheckCondition(Actor source, Ability ability);
    }
}
