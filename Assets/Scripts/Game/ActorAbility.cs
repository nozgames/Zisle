using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Defines an ability that an actor can execute to perform a function.
    /// </summary>
    [CreateAssetMenu(menuName = "Zisle/Actor Ability")]
    public class ActorAbility : NetworkScriptableObject<ActorAbility>
    {
        [Header("General")]
        [SerializeField] private TargetFinder _targetFinder = null;
        [SerializeField] private float _moveSpeed = 0.0f;

        [Space]
        [Space]
        [SerializeField] private ActorAbilityCondition[] _conditions = null;

        [Space]
        [SerializeField] private ActorCommand[] _commandsOnUse = null;

        [Space]
        [SerializeField] private ActorCommand[] _commandsOnHit = null;

        [Space]
        [SerializeField] private ActorCommand[] _commandsOnMiss = null;

        private static List<Actor> _targets = new List<Actor>(128);

        public float MoveSpeed => _moveSpeed;

        public bool Execute (Actor source)
        {
            // Check pre-target conditions
            foreach (var condition in _conditions)
                if (!condition.CheckCondition(source, this))
                    return false;

            // Use the target finder to find all possible targets
            _targets.Clear();
            if (null != _targetFinder)
                _targetFinder.FindTargets(source, _targets);

            // Check target conditions
            foreach (var condition in _conditions)
                if (!condition.CheckCondition(source, this, _targets))
                    return false;

            // Execute on use commands on ourselves
            ExecuteCommands(_commandsOnUse, source, source);

            if (_targets.Count == 0)
                ExecuteCommands(_commandsOnMiss, source, source);
            else
                foreach (var target in _targets) 
                    ExecuteCommands(_commandsOnHit, source, target);

            _targets.Clear();

            return true;
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
        /// Register unique identifiers for all referenced networked scriptable objects
        /// </summary>
        public override void RegisterNetworkId ()
        {
            base.RegisterNetworkId();

            _commandsOnHit.RegisterNetworkIds();
            _commandsOnMiss.RegisterNetworkIds();
            _commandsOnUse.RegisterNetworkIds();
        }
    }
}
