using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public class NetworkScriptableObject : ScriptableObject
    {
        private static Dictionary<ulong, NetworkScriptableObject> _networkedObjects = new Dictionary<ulong, NetworkScriptableObject>();

        public ulong NetworkId { get; private set; }

        public virtual void RegisterNetworkId ()
        {
            NetworkId = HashCode.GetStableHash64(name);
            _networkedObjects[NetworkId] = this;
        }

        public static NetworkScriptableObject Get (ulong networkId)
        {
            _networkedObjects.TryGetValue(networkId, out var obj);
            return obj;
        }

        public static T Get<T>(ulong networkId) where T : NetworkScriptableObject =>
            Get(networkId) as T;
    }
}
