using UnityEngine;

namespace NoZ.Zisle
{
    public enum TargetType
    {
        /// <summary>
        /// The effect is applied to the source 
        /// </summary>
        Self,

        /// <summary>
        /// The effect is applied to a custom set of targets defined by a target finder
        /// </summary>
        Custom,

        /// <summary>
        /// The effect is applied to all targets of the parent.  In the case of a group effect it is 
        /// the targets the group effect is being applied to and in the case of an Ability event it is 
        /// the targets of the ability event.
        /// </summary>
        Inherit
    }

    public enum ActorEffectLifetime
    {
        /// <summary>
        /// Single frame
        /// </summary>
        Frame,

        /// <summary>
        /// Duration of an ability
        /// </summary>
        Ability,

        /// <summary>
        /// Leave the effect on until another ability is used
        /// </summary>
        NextAbility,

        /// <summary>
        /// Specific amount of time
        /// </summary>
        Time,

        /// <summary>
        /// Never remove the effect
        /// </summary>
        Forever,

        /// <summary>
        /// Automatically determine the lifetime based on the effect.  For example
        /// an effect that plays a non looping visual effect would be removed when the
        /// effect stops.
        /// </summary>
        Auto,

        /// <summary>
        /// Effect will add and apply and immediately remove
        /// </summary>
        Instant,

        /// <summary>
        /// Inherit the lifetime from a parent effect.  If the effect does not have a parent effect then
        /// Auto will be used instead.
        /// </summary>
        Inherit
    }

    public abstract class ActorEffect : NetworkScriptableObject
    {
        [Header("General")]
        [Tooltip("Target Finder to use to find targets for the effect.  If null the effect will be applied to self.")]
        [SerializeField] private ActorEffectLifetime _lifetime = ActorEffectLifetime.Time;
        [SerializeField] private float _duration = 1.0f;
        [SerializeField] private TargetType _target = TargetType.Inherit;
        [SerializeField] private TargetFinder _targetFinder = null;

        public ActorEffectLifetime Lifetime
        {
            get => _lifetime;
            set => _lifetime = value;
        }

        public float Duration => _duration;

        public TargetType Target => _target;

        public TargetFinder TargetFinder => _targetFinder;

        public virtual bool DoesOverride(ActorEffect effect) => false;

        public abstract void Apply(ActorEffectContext context);

        public abstract void Remove(ActorEffectContext context);

        public virtual void Release(ActorEffectContext context) { }

        public virtual void ExecuteOnServer (Actor source, Actor target) => throw new System.NotImplementedException();

        public virtual void ExecuteOnClent(Actor source, Actor target) => throw new System.NotImplementedException();
    }
}
