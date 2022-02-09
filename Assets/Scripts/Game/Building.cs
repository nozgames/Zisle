using NoZ.Events;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public class Building : Actor
    {
        [Header("Building")]
        [SerializeField] private float _construction = 0.0f;
        
        public bool IsConstructed { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (_construction < 1.0f)
                foreach (var renderer in GetComponentsInChildren<Renderer>())
                    renderer.gameObject.GetOrAddComponent<MaterialOverride>().Override = GhostMaterial;
        }

        public void Build(float build)
        {
            if (IsConstructed)
                return;

            _construction += build;
            _construction = Mathf.Clamp01(build);

            if (_construction >= 1.0f)
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
    }
}
