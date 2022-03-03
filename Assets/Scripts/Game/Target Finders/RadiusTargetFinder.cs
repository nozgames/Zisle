using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Target Finders/Radius")]
    public class RadiusTargetFinder : TargetFinder
    {
        [SerializeField] private float _radius = 1.0f;
        [SerializeField] private ActorTypeMask _mask = ActorTypeMask.None;
        [SerializeField] private int _minCount = 0;
        [SerializeField] private int _maxCount = 1;

        private static Collider[] _colliders = new Collider[128];

        protected override void AddTargets(Actor source)
        {
            var count = Physics.OverlapSphereNonAlloc(source.transform.position, _radius, _colliders, _mask.ToLayerMask());
            if (count < _minCount)
                return;

            for (int i = 0; i < _maxCount && count > 0; i++)
            {
                var bestScore = float.MaxValue;
                var bestTarget = (Actor)null;
                var bestIndex = 0;

                for (int j = 0; j < count; j++)
                {
                    var target = _colliders[j].GetComponentInParent<Actor>();
                    if (target == null || target == source || target.IsDead)
                        continue;

                    var distSqr = target.DistanceToSqr(source);
                    if (distSqr < bestScore)
                    {
                        bestTarget = target;
                        bestIndex = j;
                    }
                }

                if (bestTarget != null)
                {
                    Add(bestTarget);
                    _colliders[bestIndex] = _colliders[count - 1];
                    count--;
                }
            }

            for (int j = 0; j < count; j++) _colliders[j] = null;
        }
    }
}
