using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Attach To Slot")]
    public class AttachToSlot : ActorEffect
    {
        [Header("Attach To Slot")]
        [SerializeField] private ActorSlot _slot = ActorSlot.None;
        [SerializeField] private PrefabPool _prefab = null;

        /// <summary>
        /// Override any effects in the same slot
        /// </summary>
        public override bool DoesOverride(ActorEffect effect) =>
            effect is AttachToSlot slot && slot._slot == _slot;

        public override void Apply(ActorEffectContext context)
        {
            if (null == _prefab)
                return;

            var slot = context.Target.GetSlotTransform(_slot);
            if (slot == null)
                return;

            context.UserData = _prefab.Instantiate(slot.transform);
        }

        public override void Remove(ActorEffectContext context) => Release(context);

        public override void Release(ActorEffectContext context)
        {
            if (context.UserData != null)
            {
                (context.UserData as GameObject).PooledDestroy();
                context.UserData = null;
            }
        }
    }
}
