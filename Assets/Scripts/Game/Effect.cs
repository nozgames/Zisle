using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Represents a single effect that can be applied to an actor.  Each effect can have multiple components 
    /// that define what the effect actually does
    /// </summary>
    [CreateAssetMenu(menuName = "Zisle/Effect")]
    public class Effect : NetworkScriptableObject<Effect>, ISerializationCallbackReceiver
    {
        [Header("General")]
        [SerializeField] private TargetType _target = TargetType.Inherit;
        [Tooltip("Target Finder to use to find targets for the effect.  If null the effect will be applied to self.")]
        [SerializeField] private TargetFinder _targetFinder = null;
        [SerializeField] private EffectLifetime _lifetime = EffectLifetime.Time;
        [SerializeField] private float _duration = 1.0f;
        [SerializeField] private int _maximumStacks = 1;
        [SerializeField] private int _priority = 0;

        [SerializeField] private EffectComponent[] _components = null;

        public EffectLifetime Lifetime
        {
            get => _lifetime;
            set => _lifetime = value;
        }

        public int ComponentCount => _components.Length;

        public EffectComponent[] Components => _components;

        public int MaximumStacks => _maximumStacks;

        public float Duration => _duration;

        public TargetType Target => _target;

        public TargetFinder TargetFinder => _targetFinder;

        public int Priority
        {
            get => _priority;
            set => _priority = value;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (_components != null)
                foreach (var component in _components)
                    component.Effect = this;
        }
    }
}
