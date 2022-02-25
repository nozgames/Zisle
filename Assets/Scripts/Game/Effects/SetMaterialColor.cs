using UnityEngine;
using NoZ.Tweening;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Actor Effects/Set Material Color")]
    public class SetMaterialColor : ActorEffect
    {
        [SerializeField] private string _name = null;
        
        [ColorUsage(true, true)]
        [SerializeField] private Color _value = Color.white;

        [SerializeField] private float _blendTime = 0.05f;

        private int _nameId = -1;

        public int NameId
        {
            get => _nameId;
            set => _nameId = value;
        }

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
            _nameId = Shader.PropertyToID(_name);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _nameId = Shader.PropertyToID(_name);
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
                    _tweenProvider = new ActorMaterialColorProvider(_nameId);

                Tween.To(_tweenProvider, context.Target, _value).Duration(_blendTime).Id(_nameId).EaseInCubic().Play();
            }
            else
                context.Target.SetMaterialColor(_nameId, _value);
        }

        public override void Remove(ActorEffectContext context)
        {
            Tween.Stop(context.Target, _nameId);
        }

        public override bool DoesOverride(ActorEffect effect) =>
            (effect is SetMaterialColor set && set._nameId == _nameId);
    }
}
