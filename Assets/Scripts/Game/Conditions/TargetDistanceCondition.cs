using UnityEngine;

namespace NoZ.Zisle
{
    public class TargetDistanceCondition : TargetCondition
    {
        [SerializeField] private float _distance = 1.0f;

        private float _distanceSqr;

        private void OnEnable()
        {
            _distanceSqr = _distance;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _distanceSqr = _distance;
        }
#endif

        protected override float CheckCondition(Actor source, Ability ability, TargetFinder targets)
        {
            foreach (var target in targets.Targets)
                if (target.DistanceTo(source) - source.Radius - target.Radius <= _distance)
                    return 1.0f;

            return 0.0f;
        }
    }
}
