using UnityEngine;
using NoZ.Tweening;

namespace NoZ.Zisle
{
    public class SetMaterialColor : EffectComponent
    {
        [SerializeField] private Tag _property = null;
        
        [ColorUsage(true, true)]
        [SerializeField] private Color _value = Color.white;

        [SerializeField] private float _blendTime = 0.05f;

        public override Tag Tag => _property;

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

        internal class ActorMaterialColorProvider : ColorProvider<Actor>
        {
            private int _propertyId;
            protected sealed override Color GetValue(Actor target) => target.GetMaterialColor(_propertyId);
            protected sealed override void SetValue(Actor target, Color value) => target.SetMaterialColor(_propertyId, value);
            public ActorMaterialColorProvider(int propertyId) => _propertyId = propertyId;
        }

        private ActorMaterialColorProvider _tweenProvider;

        public override void Apply(EffectComponentContext context)
        {
            if (_blendTime > 0.0f)
            {
                if (null == _tweenProvider)
                    _tweenProvider = new ActorMaterialColorProvider(_property.ShaderPropertyId);

                Tween.To(_tweenProvider, context.Target, _value).Duration(_blendTime).Id(_property.ShaderPropertyId).EaseInCubic().Play();
            }
            else
                context.Target.SetMaterialColor(_property.ShaderPropertyId, _value);
        }

        public override void Remove(EffectComponentContext context)
        {
            Tween.Stop(context.Target, _property.ShaderPropertyId);
        }

        public override void Release(EffectComponentContext context)
        {
        }
    }
}
