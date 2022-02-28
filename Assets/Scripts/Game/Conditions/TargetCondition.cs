using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public abstract class TargetCondition : AbilityCondition
    {
        [SerializeField] private TargetType _target = TargetType.Inherit;
        [SerializeField] private TargetFinder _targetFinder = null;

        public TargetType Target => _target;
        public TargetFinder TargetFinder => _targetFinder;

        public sealed override float CheckCondition(Actor source, Ability ability) =>
            CheckCondition(source, ability, ability.FindTargets(source));

        protected abstract float CheckCondition(Actor source, Ability ability, TargetFinder targets);
    }
}
