using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Conditions/Chained Ability")]
    public class ChainedAbilityCondition : ActorAbilityCondition
    {
        [Tooltip("List of abilities that qualify for chaining from")] 
        [SerializeField] private ActorAbility[] _previousAbility = null;

        [Tooltip("Time before the previous ability no longer qualifies")]
        [SerializeField] private float _time = 0.0f;

        public override float CheckCondition(Actor source, ActorAbility ability)
        {
            var lastUsed = source.LastAbilityUsed;
            foreach(var previous in _previousAbility)
                if(previous == lastUsed)
                    return (Time.time - source.LastAbilityUsedEndTime) <= _time ? 1.0f : 0.0f;

            return 0.0f;
        }

        public override float CheckCondition(Actor source, ActorAbility ability, List<Actor> targets) => 1.0f;
    }
}
