using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Effects/Instantiate Prefab")]
    public class InstantiatePrefab : ActorEffect
    {
        [SerializeField] private PrefabPool _prefab = null;
        [SerializeField] private ActorSlot _slot = ActorSlot.None;
        [SerializeField] private bool _parentToSlot = true;

        /// <summary>
        /// Override any effects in the same slot
        /// </summary>
        public override bool DoesOverride(ActorEffect effect) =>
            effect is InstantiatePrefab slot && slot._slot == _slot;

        public override void Apply(ActorEffectContext context)
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

        public override void Remove(ActorEffectContext context)
        {
            if (context.UserData != null)
                (context.UserData as GameObject).SetActive(false);
        }

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
