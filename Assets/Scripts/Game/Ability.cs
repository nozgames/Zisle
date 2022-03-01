using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("com.noz.zisle.editor")]

namespace NoZ.Zisle
{
    [CreateAssetMenu (menuName = "Zisle/Ability")]
    public class Ability : NetworkScriptableObject<Ability>
    {
        [SerializeField] private Animations.AnimationShader _animation = null;
        [SerializeField] private float _moveSpeed = 0.0f;

        [SerializeField] private TargetType _target = TargetType.Custom;
        [SerializeField] private TargetFinder _targetFinder = null;

        [SerializeField] private AbilityEvent[] _events;

        [SerializeField] private AbilityCondition[] _conditions;

        public IEnumerable<AbilityEvent> Events => _events;

        public IEnumerable<AbilityCondition> Conditions => _conditions;

        public TargetType Target => _target;
        public TargetFinder TargetFinder => _targetFinder;

        public float MoveSpeed => _moveSpeed;

        public Animations.AnimationShader Animation => _animation;

        public TargetFinder FindTargets(Actor source) => TargetFinder.FindTargets(source, _target, _targetFinder, null);

        public void OnEvent (Actor source, NoZ.Animations.AnimationEvent evt, TargetFinder targetFinder)
        {
            foreach(var abilityEvent in _events)
                if(abilityEvent.Event == evt)
                {
                    Execute(source, abilityEvent, targetFinder);
                    break;
                }
        }

        private void Execute (Actor source, AbilityEvent abilityEvent, TargetFinder abilityTargetFinder)
        {
            // TODO: inherit targets?  The only way that would be possible is to them in the actor

            foreach(var effect in abilityEvent.Effects)
            {
                if (effect == null)
                    continue;

                var effectTargetFinder = TargetFinder.FindTargets(source, effect.Target, effect.TargetFinder, abilityTargetFinder);
                if (effectTargetFinder == null)
                    continue;

                foreach(var target in effectTargetFinder.Targets)
                    target.AddEffect(source, effect);
            }
        }

        public float CalculateScore(Actor source, List<Actor> targetCache)
        {
            var score = 1.0f;

            if (_conditions == null)
                return 1.0f;

            foreach (var condition in _conditions)
            {
                score *= condition.CheckCondition(source, this);
                if (score <= float.Epsilon)
                    return 0.0f;
            }

            return score;
        }

        public override void RegisterNetworkId()
        {
            base.RegisterNetworkId();

            if (_events != null)
                foreach(var evt in _events)
                    if(evt != null)
                        evt.RegisterNetworkId();
        }
    }
}
