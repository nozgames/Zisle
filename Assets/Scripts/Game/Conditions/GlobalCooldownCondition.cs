using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Conditions/Global Cooldown")]
    public class GlobalCooldownCondition : ActorAbilityCondition
    {
        [SerializeField] private float _duration = 1.5f;

        public override float CheckCondition(Actor source, ActorAbility ability) =>
            (Time.time - source.LastAbilityUsedTime) >= _duration ? 1.0f : 0.0f;

        public override float CheckCondition(Actor source, ActorAbility ability, List<Actor> targets) => 1.0f;
    }
}
