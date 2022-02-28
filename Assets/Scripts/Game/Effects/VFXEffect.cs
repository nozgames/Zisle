using UnityEngine;
using UnityEngine.VFX;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Play VFX")]
    public class VFXEffect : ActorEffect
    {
        [SerializeField] private VisualEffectAsset _vfx = null;
        [SerializeField] private ActorSlot _slot = ActorSlot.None;

        public override void Apply(ActorEffectContext context)
        {
            context.UserData = VFXManager.Instance.Play(_vfx, context.Target.GetSlotTransform(_slot));
        }

        public override void Remove(ActorEffectContext context)
        {
            // TODO: should we let it finish first?
            Release(context);
        }

        public override void Release(ActorEffectContext context)
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
