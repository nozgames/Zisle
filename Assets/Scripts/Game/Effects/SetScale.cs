using UnityEngine;
using NoZ.Tweening;

namespace NoZ.Zisle
{
    public class SetScale : EffectComponent
    {
        [SerializeField] private ActorSlot _slot = ActorSlot.None;
        [SerializeField] private Vector3 _value = Vector3.one;
        [SerializeField] private float _blendTime = 0.05f;

        public override Tag Tag => _slot switch
        {
            ActorSlot.None => TagManager.Instance.SetScaleRoot,
            ActorSlot.RightHand => TagManager.Instance.SetScaleRightHand,
            ActorSlot.LeftHand => TagManager.Instance.SetScaleLeftHand,
            ActorSlot.Body => TagManager.Instance.SetScaleBody,
            _ => throw new System.NotImplementedException()
        };

        public ActorSlot Slot
        {
            get => _slot;
            set => _slot = value;
        }

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

        public override void Apply(EffectComponentContext context)
        {
            if(_slot == ActorSlot.None)
            {
                if (_blendTime > 0.0f)
                    context.Target.TweenVector("VisualScale", _value).Duration(_blendTime).EaseInCubic().Play();
                else
                    context.Target.VisualScale = _value;
            }
            else
            {
                var slotTransform = context.Target.GetSlotTransform(_slot);
                if (slotTransform == null)
                    return;

                if (_blendTime > 0.0f)
                    slotTransform.TweenLocalScale(_value).Duration(_blendTime).EaseInCubic().Play();
                else
                    slotTransform.localScale = _value;

            }
        }

        public override void Remove(EffectComponentContext context)
        {
        }

        public override void Release(EffectComponentContext context)
        {
        }
    }
}
