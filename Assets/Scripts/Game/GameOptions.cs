using NoZ.Events;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Synchronized game options
    /// </summary>
    public class GameOptions : NetworkBehaviour
    {
        [Header("General")]
        [SerializeField] private int _maxIslands = 4;
        [SerializeField] private int _startingLanes = 1;
        
        [Header("Path Weights")]
        [SerializeField] public float _pathWeight0 = 0.1f;
        [SerializeField] public float _pathWeight1 = 1.0f;
        [SerializeField] public float _pathWeight2 = 0.4f;
        [SerializeField] public float _pathWeight3 = 0.2f;


        private NetworkVariable<int> _startingLanesSync = new NetworkVariable<int>(1);

        public int StartingLanes
        {
            get => _startingLanes;
            set
            {
                if (_startingLanes == value) return;
                
                _startingLanes = value;

                if(NetworkManager.Singleton != null)
                    SetStartingLanesServerRpc(value);
            }
        }

        public override void OnNetworkSpawn ()
        {
            base.OnNetworkDespawn();

            name = "GameOptions";

            _startingLanesSync.OnValueChanged += (p, n) => GameEvent.Raise(this, new GameOptionStartingLanesChanged { Options = this, OldValue = p, NewValue = n });

            GameEvent.Raise(this, new GameOptionsSpawned { Options = this });
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            GameEvent.Raise(this, new GameOptionsDespawned { Options = this });
        }

        [ServerRpc(RequireOwnership = false)] void SetStartingLanesServerRpc (int lanes)
        {
            _startingLanes = lanes;
            _startingLanesSync.Value = Mathf.Clamp(lanes, 1, 4);
        }        

        public WorldGenerator.Options ToGeneratorOptions() =>
            new WorldGenerator.Options
            {
                StartingLanes = _startingLanes,
                MaxIslands = _maxIslands,
                PathWeight0 = _pathWeight0,
                PathWeight1 = _pathWeight1,
                PathWeight2 = _pathWeight2,
                PathWeight3 = _pathWeight3
            };
    }
}
