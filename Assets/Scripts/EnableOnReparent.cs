using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public class EnableOnReparent : NetworkBehaviour
    {
        [SerializeField] private MonoBehaviour[] _behaviours;

        public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
        {
            base.OnNetworkObjectParentChanged(parentNetworkObject);

            foreach(var behaviour in _behaviours)
                behaviour.enabled = true;
        }
    }
}
