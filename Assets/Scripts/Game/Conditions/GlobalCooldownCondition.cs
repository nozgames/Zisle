using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Conditions/Global Cooldown")]
    public class GlobalCooldownCondition : ActorAbilityCondition
    {
        [SerializeField] private float _duration = 1.5f;

        public override bool CheckCondition(Actor source, ActorAbility ability) =>
            (Time.time - source.LastAbilityUsedTime) >= _duration;

        public override bool CheckCondition(Actor source, ActorAbility ability, List<Actor> targets) => true;
    }
}
