using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Conditions/Target Count")]
    public class TargetCountCondition : ActorAbilityCondition
    {
        [SerializeField] private int _minTargetCount = 0;
        [SerializeField] private int _maxTargetCount = 128;

        public override float CheckCondition(Actor source, ActorAbility ability) => 1.0f;

        public override float CheckCondition(Actor source, ActorAbility ability, List<Actor> targets) =>
            (targets.Count >= _minTargetCount && targets.Count <= _maxTargetCount) ? 1.0f : 0.0f;
    }
}
