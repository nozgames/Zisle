using UnityEngine;
using NoZ.Tweening;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Set Material Color")]
    public class SetMaterialColor : ActorEffect
    {
        [SerializeField] private string _propertyName = "_Color";
        
        [ColorUsage(true, true)]
        [SerializeField] private Color _value = Color.white;

        [SerializeField] private float _blendTime = 0.05f;

        public int PropertyNameId { get; set; }

        public Color Value
        {
            get => _value;
            set => _value = value;
        }

        public float BlendTime
        {
            get => _blendTime;
            set => _blendTime = value;
        }

        private void OnEnable()
        {
            PropertyNameId = Shader.PropertyToID(_propertyName);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            PropertyNameId = Shader.PropertyToID(_propertyName);
        }
#endif

        internal class ActorMaterialColorProvider : ColorProvider<Actor>
        {
            private int _propertyId;
            protected sealed override Color GetValue(Actor target) => target.GetMaterialColor(_propertyId);
            protected sealed override void SetValue(Actor target, Color value) => target.SetMaterialColor(_propertyId, value);
            public ActorMaterialColorProvider(int propertyId) => _propertyId = propertyId;
        }

        private ActorMaterialColorProvider _tweenProvider;

        public override void Apply(ActorEffectContext context)
        {
            if (_blendTime > 0.0f)
            {
                if (null == _tweenProvider)
                    _tweenProvider = new ActorMaterialColorProvider(PropertyNameId);

                Tween.To(_tweenProvider, context.Target, _value).Duration(_blendTime).Id(PropertyNameId).EaseInCubic().Play();
            }
            else
                context.Target.SetMaterialColor(PropertyNameId, _value);
        }

        public override void Remove(ActorEffectContext context)
        {
            Tween.Stop(context.Target, PropertyNameId);
        }

        public override bool DoesOverride(ActorEffect effect) =>
            (effect is SetMaterialColor set && set.PropertyNameId == PropertyNameId);
    }
}
