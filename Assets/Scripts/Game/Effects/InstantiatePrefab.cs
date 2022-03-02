using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public class InstantiatePrefab : EffectComponent
    {
        [SerializeField] private PrefabPool _prefab = null;
        [SerializeField] private ActorSlot _slot = ActorSlot.None;
        [SerializeField] private bool _parentToSlot = true;
        [SerializeField] private Tag _tag = null;

        public override Tag Tag => _tag;

        public override bool ApplyOnClient => _prefab != null && !_prefab.IsNetworkObject;

        public override void Apply(EffectComponentContext context)
        {
            // If the prefab was already instantiated then just enable it
            if (context.UserData != null)
            {
                (context.UserData as GameObject).SetActive(true);
                return;
            }

            if (null == _prefab)
                return;

            var slot = context.Target.GetSlotTransform(_slot);
            if (slot == null)
                return;

            var go = _prefab.Instantiate(_parentToSlot ? slot.transform : Game.Instance.transform);
            go.transform.position = slot.transform.position;
            go.transform.rotation = slot.transform.rotation;
            go.SetActive(true);
            context.UserData = go;
        }

        public override void Remove(EffectComponentContext context)
        {
            if (context.UserData != null)
                (context.UserData as GameObject).SetActive(false);
        }

        public override void Release(EffectComponentContext context)
        {
            if (context.UserData != null)
            {
                (context.UserData as GameObject).PooledDestroy();
                context.UserData = null;
            }
        }
    }
}
