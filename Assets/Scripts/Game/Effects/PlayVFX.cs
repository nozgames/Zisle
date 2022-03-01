using UnityEngine;
using UnityEngine.VFX;

namespace NoZ.Zisle
{
    public class PlayVFX : EffectComponent
    {
        [SerializeField] private VisualEffectAsset _vfx = null;
        [SerializeField] private ActorSlot _slot = ActorSlot.None;
        [SerializeField] private Tag _tag = null;

        public override Tag Tag => _tag;

        public override void Apply(EffectComponentContext context)
        {
            context.UserData = VFXManager.Instance.Play(_vfx, context.Target.GetSlotTransform(_slot));
        }

        public override void Remove(EffectComponentContext context)
        {
            Release(context);
        }

        public override void Release(EffectComponentContext context)
        {
            if(context.UserData != null)
            {
                var vfx = context.UserData as VisualEffect;
                VFXManager.Instance.Release(context.UserData as VisualEffect);
                context.UserData = null;
            }                
        }
    }
}
