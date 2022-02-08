using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Conditions/Cooldown")]
    public class CooldownCondition : ActorAbilityCondition
    {
        [SerializeField] private float _duration = 1.0f;

        public override bool CheckCondition(Actor source, ActorAbility ability) => 
            (Time.time - source.GetAbilityLastUsedTime(ability)) >= _duration;

        public override bool CheckCondition(Actor source, ActorAbility ability, List<Actor> targets) => true;        
    }
}
