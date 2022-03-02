using UnityEngine;

namespace NoZ.Zisle
{
    public class TargetTypeCondition : TargetCondition
    {
        [SerializeField] private ActorTypeMask _types = ActorTypeMask.Enemy;

        protected override float CheckCondition(Actor source, Ability ability, TargetFinder targets)
        {
            foreach (var target in targets.Targets)
                if ((target.TypeMask & _types) != 0)
                    return 1.0f;

            return 0.0f;
        }
    }
}
