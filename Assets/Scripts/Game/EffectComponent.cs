using UnityEngine;

namespace NoZ.Zisle
{
    public abstract class EffectComponent : ScriptableObject
    {
        private Effect _effect;

        /// <summary>
        /// Optionally tag a component which will make it mutulally exclusive 
        /// with any other component with the same tag. 
        /// </summary>
        public virtual Tag Tag => null;

        public Effect Effect
        {
            get => _effect;
            set
            {
                _effect = value;
            }
        }

        public abstract void Apply(EffectComponentContext context);

        public abstract void Remove(EffectComponentContext context);

        public abstract void Release(EffectComponentContext context);
    }
}
