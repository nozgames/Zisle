using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
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
        Auto
    }

    public abstract class ActorEffect : NetworkScriptableObject
    {
        [Header("General")]
        [SerializeField] private ActorEffectLifetime _lifetime = ActorEffectLifetime.Frame;
        [SerializeField] private float _duration = 1.0f;

        public ActorEffectLifetime Lifetime
        {
            get => _lifetime;
            set => _lifetime = value;
        }

        public float Duration => _duration;

        public virtual bool DoesOverride(ActorEffect effect) => false;

        public abstract void Apply(ActorEffectContext context);

        public abstract void Remove(ActorEffectContext context);

        public virtual void Release(ActorEffectContext context) { }

        public virtual void ExecuteOnServer (Actor source, Actor target) => throw new System.NotImplementedException();

        public virtual void ExecuteOnClent(Actor source, Actor target) => throw new System.NotImplementedException();
    }
}
