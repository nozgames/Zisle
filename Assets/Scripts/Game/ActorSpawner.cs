using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public class ActorSpawner : MonoBehaviour
    {
        [SerializeField] private ActorDefinition[] _actorDefinitions = null;

        public void Spawn()
        {
            if(_actorDefinitions != null && _actorDefinitions.Length > 0)
                _actorDefinitions[Random.Range(0, _actorDefinitions.Length)].Spawn(transform.position, transform.rotation);
        }
    }
}
