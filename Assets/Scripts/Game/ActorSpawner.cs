using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public class ActorSpawner : MonoBehaviour
    {
        [SerializeField] private ActorDefinition[] _actorDefinitions = null;

        public void Spawn()
        {
            if(_actorDefinitions != null)
            {
                var def = _actorDefinitions[Random.Range(0, _actorDefinitions.Length)];
                Instantiate(def.Prefab, transform.position, transform.rotation).GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
