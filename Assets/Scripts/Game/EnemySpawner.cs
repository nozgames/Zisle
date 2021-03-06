using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float _minRate = 0.1f;
        [SerializeField] private float _maxRate = 0.4f;

        private ActorDefinition[] _actorDefinitions = null;
        private float _nextSpawn;

        private void Start()
        {
            _actorDefinitions = GameManager.Instance.ActorDefinitions.Where(d => d.ActorType == ActorType.Enemy).ToArray();
            _nextSpawn = Time.time + (1.0f / Random.Range(_minRate, _maxRate));
        }

        private void Update()
        {
            if (!NetworkManager.Singleton.IsHost)
                return;

            if (Time.time < _nextSpawn)
                return;

            _nextSpawn = Time.time + (1.0f / Random.Range(_minRate, _maxRate));

            var def = _actorDefinitions[Random.Range(0, _actorDefinitions.Length)];
            var actor = Instantiate(def.Prefab, transform.position, transform.rotation).GetComponent<Actor>();
            actor.NetworkObject.Spawn();
        }
    }
}
