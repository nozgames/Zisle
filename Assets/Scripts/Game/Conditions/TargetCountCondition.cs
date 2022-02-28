using UnityEngine;

namespace NoZ.Zisle
{
    public class TargetCountCondition : TargetCondition
    {
        [SerializeField] private int _minTargetCount = 0;
        [SerializeField] private int _maxTargetCount = 128;

        protected override float CheckCondition(Actor source, Ability ability, TargetFinder targets) =>
            (targets.Count >= _minTargetCount && targets.Count <= _maxTargetCount) ? 1.0f : 0.0f;
    }
}
