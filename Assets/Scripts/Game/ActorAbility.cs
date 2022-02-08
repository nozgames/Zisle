using NoZ.Animations;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Actor Ability")]
    public class ActorAbility : NetworkScriptableObject<ActorAbility>
    {
        [Header("Target")]
        [SerializeField] private int _targetCount = 1;
        [SerializeField] private float _targetRange = 0.5f;
        [SerializeField] private float _targetArc = 45.0f;
        [SerializeField] private LayerMask _targetMask = -1;

        [Header("Commands")]
        [SerializeField] private ActorCommand[] _commandsOnUse = null;
        [SerializeField] private ActorCommand[] _commandsOnHit = null;
        [SerializeField] private ActorCommand[] _commandsOnMiss = null;

        private static Collider[] _colliders = new Collider[128];
        private static List<Actor> _targets = new List<Actor>(128);

        private float _targetArcScore;
        private float _targetArcCos;

        private void OnEnable()
        {
            _targetArcCos = Mathf.Cos(_targetArc * Mathf.Deg2Rad);
            _targetArcScore = 1.0f / (1.0f - _targetArcCos);
        }

        public void Execute (Actor source)
        {
            FindTargets(source);

            // Execute on use commands on ourselves
            ExecuteCommands(_commandsOnUse, source, source);

            if (_targets.Count == 0)
                ExecuteCommands(_commandsOnMiss, source, source);
            else
                foreach (var target in _targets) ExecuteCommands(_commandsOnHit, source, target);

            _targets.Clear();
        }

        /// <summary>
        /// Execute all commands from the given list on the target
        /// </summary>
        /// <param name="commands">Commands to execute</param>
        /// <param name="source">Source actor</param>
        /// <param name="target">Target actor</param>
        private void ExecuteCommands (ActorCommand[] commands, Actor source, Actor target)
        {
            foreach (var command in commands)
                target.ExecuteCommand(command, source);
        }

        /// <summary>
        /// Find all targets for the ability from the given source
        /// </summary>
        /// <param name="source">Source actor</param>
        /// <returns>Number of targets hit</returns>
        private int FindTargets (Actor source)
        {
            _targets.Clear();

            var count = Physics.OverlapSphereNonAlloc(source.transform.position, _targetRange, _colliders, _targetMask);
            var forward = source.transform.forward.ZeroY();
            for(int i = 0; i <_targetCount; i++)
            {
                var bestScore = float.MaxValue;
                var bestTarget = (Actor)null;
                var bestIndex = 0;

                for(int j=0; j<count; j++)
                {
                    var target = _colliders[j].GetComponentInParent<Actor>();
                    if (target == null || target == source)
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
                    _targets.Add(bestTarget);
                    _colliders[bestIndex] = _colliders[_colliders.Length - 1];
                    count--;
                    break;
                }
            }

            for (int j = 0; j < count; j++) _colliders[j] = null;

            return _targets.Count;
        }

        public override void RegisterNetworkId ()
        {
            base.RegisterNetworkId();

            _commandsOnHit.RegisterNetworkIds();
            _commandsOnMiss.RegisterNetworkIds();
            _commandsOnUse.RegisterNetworkIds();
        }
    }
}
