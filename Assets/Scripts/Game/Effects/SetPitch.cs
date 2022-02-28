using UnityEngine;
using NoZ.Tweening;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Set Pitch")]
    public class SetPitch : ActorEffect
    {
        private const int TweenId = int.MaxValue - 1;

        [SerializeField] private float _value = 0.0f;
        [SerializeField] private float _blendTime = 0.05f;

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

        public override void Apply(ActorEffectContext context)
        {
            if (_blendTime > 0.0f)
                context.Target.TweenFloat("VisualPitch", _value).Duration(_blendTime).EaseInCubic().Id(TweenId).Play();
            else
                context.Target.VisualPitch = _value;
        }

        public override void Remove(ActorEffectContext context)
        {
            Tween.Stop(context.Target, TweenId);
        }

        public override bool DoesOverride(ActorEffect effect) =>
            effect is SetPitch;
    }
}
