using UnityEngine;

namespace NoZ.Zisle
{
    public class GlobalCooldownCondition : AbilityCondition
    {
        public override float CheckCondition(Actor source, Ability ability) =>
            (Time.time - source.LastAbilityUsedTime) >= source.Definition.GlobalCooldown ? 1.0f : 0.0f;
    }
}
