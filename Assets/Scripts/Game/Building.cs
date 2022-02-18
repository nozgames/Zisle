using NoZ.Events;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public class Building : Actor
    {
        public virtual bool CanConstruct => true;

        public bool IsConstructed => !IsDamaged;

        public override void Heal(Actor source, float heal)
        {
            if (IsConstructed || !CanConstruct)
                return;

            base.Heal(source, heal);

            if(IsConstructed)
            {
                CanHit = false;

                OnConstructedServer();
                SendConstructedEventClientRpc();
            }
        }
    
        [ClientRpc]
        private void SendConstructedEventClientRpc ()
        {
            GameEvent.Raise(this, new BuildingConstructed { });

            foreach (var renderer in GetComponentsInChildren<Renderer>())
                renderer.gameObject.GetOrAddComponent<MaterialOverride>().Override = null;

            OnConstructedClient();
        }

        protected virtual void OnConstructedServer() { }

        protected virtual void OnConstructedClient() { }

        public override void UpdateAttributes()
        {
            base.UpdateAttributes();

            // Start health at 1 because building will heal 
            Health = 1.0f;

            if (IsDamaged)
                foreach (var renderer in GetComponentsInChildren<Renderer>())
                    renderer.gameObject.GetOrAddComponent<MaterialOverride>().Override = GhostMaterial;
        }
    }
}
