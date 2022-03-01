using UnityEngine;

namespace NoZ.Zisle
{
    public class AbilityEvent : NetworkScriptableObject<AbilityEvent>
    {
        [SerializeField] private NoZ.Animations.AnimationEvent _event;
        [SerializeField] private Effect[] _effects = null;

        public Animations.AnimationEvent Event
        {
            get => _event;
            set => _event = value;
        }
        public Effect[] Effects => _effects;
        
        public override void RegisterNetworkId()
        {
            base.RegisterNetworkId();

            if (_effects != null)
                foreach (var effect in _effects)
                    if(effect != null)
                        effect.RegisterNetworkId();
        }
    }
}
