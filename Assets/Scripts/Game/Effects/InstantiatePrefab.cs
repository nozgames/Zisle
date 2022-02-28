using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Commands/Instantiate Prefab")]
    public class InstantiatePrefab : ActorCommand, IExecuteOnClient, IExecuteOnServer
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private bool _parentToTarget = false;

        public void ExecuteOnClient(Actor source, Actor target)
        {
            if (_prefab.TryGetComponent<NetworkObject>(out var netobj))
                return;

            Instantiate(_prefab, _parentToTarget ? target.transform : null, false);
        }

        public void ExecuteOnServer(Actor source, Actor target)
        {
            if (!_prefab.TryGetComponent<NetworkObject>(out var netobj))
                return;

            var obj = Instantiate(_prefab, null, false);
            if (obj == null)
                return;

            if (obj.TryGetComponent<NetworkObject>(out netobj))
            {
                netobj.Spawn();

                if(_parentToTarget)
                    netobj.TrySetParent(target.NetworkObject);
            }
        }
    }
}
