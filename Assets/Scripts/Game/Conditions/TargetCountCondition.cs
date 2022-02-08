using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Conditions/Target Count")]
    public class TargetCountCondition : ActorAbilityCondition
    {
        [SerializeField] private int _minTargetCount = 0;
        [SerializeField] private int _maxTargetCount = 128;

        public override bool CheckCondition(Actor source, ActorAbility ability) => true;

        public override bool CheckCondition(Actor source, ActorAbility ability, List<Actor> targets) =>
            targets.Count >= _minTargetCount && targets.Count <= _maxTargetCount;
    }
}
