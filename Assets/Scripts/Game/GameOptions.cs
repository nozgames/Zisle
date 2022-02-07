using NoZ.Events;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    public class GameOptions : NetworkBehaviour
    {
        public int MaxIslands = 64;
        public bool SpawnEnemies = true;

        [Header("Path Weights")]
        public float PathWeight0 = 0.1f;
        public float PathWeight1 = 1.0f;
        public float PathWeight2 = 0.4f;
        public float PathWeight3 = 0.2f;

        public IEnumerable<float> GetForkWeights()
        {
            yield return PathWeight0;
            yield return PathWeight1;
            yield return PathWeight2;
            yield return PathWeight3;
        }

        private NetworkVariable<int> _startingLanes = new NetworkVariable<int>(1);

        public int StartingLanes
        {
            get => _startingLanes.Value;
            set
            {
                if (_startingLanes.Value == value)
                    return;
                SetStartingLanesServerRpc(value);
            }
        }

        public override void OnNetworkSpawn ()
        {
            base.OnNetworkDespawn();

            _startingLanes.OnValueChanged += (p, n) => GameEvent.Raise(this, new GameOptionStartingLanesChanged { Options = this, OldValue = p, NewValue = n });

            GameEvent.Raise(this, new GameOptionsSpawned { Options = this });
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            GameEvent.Raise(this, new GameOptionsDespawned { Options = this });
        }

        [ServerRpc(RequireOwnership = false)] void SetStartingLanesServerRpc (int lanes) => _startingLanes.Value = Mathf.Clamp(lanes, 1, 4);
    }
}
