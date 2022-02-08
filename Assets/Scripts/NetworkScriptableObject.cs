using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public class NetworkScriptableObject : ScriptableObject
    {
        public static T Get<T>(ushort networkId) where T : NetworkScriptableObject =>
            NetworkScriptableObject<T>.Get(networkId);
    }

    public class NetworkScriptableObject<T> : NetworkScriptableObject where T : class
    {
        private static List<T> _networkedObjects = new List<T> (1024);

        public ushort NetworkId { get; private set; }

        public virtual void RegisterNetworkId ()
        {
            _networkedObjects.Add(this as T);
            NetworkId = (ushort)(_networkedObjects.Count);
        }

        public static T Get(ushort networkId) => networkId == 0 ? null : _networkedObjects[networkId - 1];
    }

    public static class NetworkScriptableObjectExtensions
    {
        public static void RegisterNetworkIds<T>(this T[] objects) where T : NetworkScriptableObject<T>
        {
            foreach (var def in objects)
                def.RegisterNetworkId();
        }
    }
}
