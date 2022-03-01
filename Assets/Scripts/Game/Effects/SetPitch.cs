using UnityEngine;
using NoZ.Tweening;

namespace NoZ.Zisle
{
    public class SetPitch : EffectComponent
    {
        private const int TweenId = int.MaxValue - 1;

        [SerializeField] private float _value = 0.0f;
        [SerializeField] private float _blendTime = 0.05f;

        public override Tag Tag => TagManager.Instance.SetPitch;

        public float Value
        {
            get => _value;
            set => _value = value;
        }

        public float BlendTime
        {
            get => _blendTime;
            set => _blendTime = value;
        }

        public override void Apply(EffectComponentContext context)
        {
            if (_blendTime > 0.0f)
                context.Target.TweenFloat("VisualPitch", _value).Duration(_blendTime).EaseInCubic().Id(TweenId).Play();
            else
                context.Target.VisualPitch = _value;
        }

        public override void Remove(EffectComponentContext context)
        {
            Tween.Stop(context.Target, TweenId);
        }

        public override void Release(EffectComponentContext context)
        {
        }
    }
}
