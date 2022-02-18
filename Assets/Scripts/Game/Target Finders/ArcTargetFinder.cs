using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Target Finders/Arc")]
    public class ArcTargetFinder : TargetFinder
    {
        [SerializeField] private int _targetCount = 1;
        [SerializeField] private float _targetRange = 0.5f;
        [SerializeField] private float _targetArc = 45.0f;
        [SerializeField] private ActorTypeMask _targetMask = ActorTypeMask.None;

        private static Collider[] _colliders = new Collider[128];

        private float _targetArcScore;
        private float _targetArcCos;

        private void OnEnable()
        {
            _targetArcCos = Mathf.Cos(_targetArc * Mathf.Deg2Rad);
            _targetArcScore = 1.0f / (1.0f - _targetArcCos);
        }

        public override void FindTargets(Actor source, List<Actor> targets)
        {
            targets.Clear();

            var count = Physics.OverlapSphereNonAlloc(source.transform.position, _targetRange, _colliders, _targetMask.ToLayerMask());
            var forward = source.transform.forward.ZeroY();
            for (int i = 0; i < _targetCount && count> 0; i++)
            {
                var bestScore = float.MaxValue;
                var bestTarget = (Actor)null;
                var bestIndex = 0;

                for (int j = 0; j < count; j++)
                {
                    var target = _colliders[j].GetComponentInParent<Actor>();
                    if (target == null || target == source || target.IsDead)
                        continue;

                    var delta = (target.transform.position - source.transform.position).ZeroY();
                    var dot = Vector3.Dot(forward, delta.normalized);
                    if (dot < _targetArcCos)
                        continue;

                    var score = ((1.0f - dot) + 0.1f) * _targetArcScore * (delta.magnitude / _targetRange);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestTarget = target;
                        bestIndex = j;
                    }
                }

                if (bestTarget != null)
                {
                    targets.Add(bestTarget);
                    _colliders[bestIndex] = _colliders[count - 1];
                    count--;
                }
            }

            for (int j = 0; j < count; j++) _colliders[j] = null;
        }
    }
}
