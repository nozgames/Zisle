using UnityEngine;
using NoZ.Tweening;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Set Scale")]
    public class SetScale : ActorEffect
    {
        [SerializeField] private Vector3 _value = Vector3.one;
        [SerializeField] private float _blendTime = 0.05f;

        public Vector3 Value
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
                context.Target.TweenVector("VisualScale", _value).Duration(_blendTime).EaseInCubic().Play();
            else
                context.Target.VisualScale = _value;
        }

        public override void Remove(ActorEffectContext context)
        {
        }

        public override bool DoesOverride(ActorEffect effect) =>
            effect is SetScale;
    }
}
