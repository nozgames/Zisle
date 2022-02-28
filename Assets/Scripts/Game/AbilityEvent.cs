using UnityEngine;

namespace NoZ.Zisle
{
    public class AbilityEvent : ScriptableObject
    {
        [SerializeField] private NoZ.Animations.AnimationEvent _event;
        [SerializeField] private TargetType _target = TargetType.Inherit;
        [SerializeField] private TargetFinder _targetFinder = null;
        [SerializeField] private ActorEffect[] _effects = null;

        public Animations.AnimationEvent Event
        {
            get => _event;
            set => _event = value;
        }
        public ActorEffect[] Effects => _effects;
        public TargetType Target => _target;
        public TargetFinder TargetFinder => _targetFinder;
    }
}
