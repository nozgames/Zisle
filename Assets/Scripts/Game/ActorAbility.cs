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
#if false
        [Header("General")]
        [SerializeField] private ActorState _actorState = ActorState.Active;
        [SerializeField] private TargetFinder _targetFinder = null;
        [SerializeField] private float _moveSpeed = 0.0f;

        [Space]
        [Space]
        [SerializeField] private AbilityCondition[] _conditions = null;

        [Space]
        [SerializeField] private ActorCommand[] _commandsOnUse = null;

        [Space]
        [SerializeField] private ActorCommand[] _commandsOnHit = null;

        [Space]
        [SerializeField] private ActorCommand[] _commandsOnMiss = null;

        public float MoveSpeed => _moveSpeed;

        public bool Execute (Actor source, List<Actor> targetCache)
        {
            // Execute on use commands on ourselves
            ExecuteCommands(_commandsOnUse, source, source);

            if (targetCache.Count == 0)
                ExecuteCommands(_commandsOnMiss, source, source);
            else
                foreach (var target in targetCache) 
                    ExecuteCommands(_commandsOnHit, source, target);

            targetCache.Clear();

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

        public float CalculateScore (Actor source, List<Actor> targetCache)
        {
            // Make sure the actor is in the correct state.
            if (source.State != _actorState)
                return 0.0f;

            // Check pre-target conditions
            var score = 1.0f;
            foreach (var condition in _conditions)
            {
                score *= condition.CheckCondition(source, this);
                if (score <= float.Epsilon)
                    return 0.0f;
            }

            // Use the target finder to find all possible targets
            targetCache.Clear();
            if (null != _targetFinder)
                _targetFinder.FindTargets(source, targetCache);

            // Check target conditions
            foreach (var condition in _conditions)
            {
                score *= condition.CheckCondition(source, this, targetCache);
                if (score <= float.Epsilon)
                    return 0.0f;
            }

            return score;
        }
#endif
    }
}
