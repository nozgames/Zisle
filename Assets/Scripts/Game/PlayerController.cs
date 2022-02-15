using System.Linq;
using NoZ.Events;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Manages the higher level control of the player
    /// </summary>
    public class PlayerController : NetworkBehaviour
    {
        private NetworkVariable<ushort> _playerClassId = new NetworkVariable<ushort>();
        private NetworkVariable<bool> _ready = new NetworkVariable<bool>();

        private Player _player = null;

        public bool IsDisconnecting { get; private set; }

        public Player Player => _player;

        /// <summary>
        /// True if the player is ready to start the game
        /// </summary>
        public bool IsReady
        {
            get => _ready.Value && !IsDisconnecting;
            set
            {
                if (!IsLocalPlayer || _ready.Value == value)
                    return;

                SetReadyServerRpc(value);
            }
        }

        public ActorDefinition PlayerClass => NetworkScriptableObject.Get<ActorDefinition>(PlayerClassId);

        public ushort PlayerClassId
        {
            get => _playerClassId.Value;
            set
            {
                if (!IsLocalPlayer || _playerClassId.Value == value)
                    return;

                SetPlayerClassIdServerRpc(value);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            name = $"PlayerController{OwnerClientId}";

            GameEvent<PlayerSpawned>.OnRaised += OnPlayerSpawned;
            GameEvent<PlayerDespawned>.OnRaised += OnPlayerDespawned;

            GameEvent.Raise(this, new PlayerConnected { PlayerController = this});

            // Tell anyone who cares that our name changed
            _ready.OnValueChanged += (p, n) => GameEvent.Raise(this, new PlayerReadyChanged { PlayerController = this });
            _playerClassId.OnValueChanged += (p,n) => GameEvent.Raise(this, new PlayerClassChanged { PlayerController = this });

            // Send the player options
            if (IsLocalPlayer)
                SetPlayerClassIdServerRpc(GameManager.Instance.ActorDefinitions.Where(d => d.ActorType == ActorType.Player).FirstOrDefault().NetworkId);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            IsDisconnecting = true;

            GameEvent<PlayerSpawned>.OnRaised -= OnPlayerSpawned;
            GameEvent<PlayerDespawned>.OnRaised -= OnPlayerDespawned;

            GameEvent.Raise(this, new PlayerDisconnected { PlayerController = this });
        }

        private void OnPlayerDespawned(object sender, PlayerDespawned evt)
        {
            if (evt.Player.OwnerClientId == this.OwnerClientId)
            {
                _player = null;
                Debug.Log("Local PlayerController disconnected from Player");
            }
        }

        private void OnPlayerSpawned(object sender, PlayerSpawned evt)
        {
            if (evt.Player.OwnerClientId == this.OwnerClientId)
            {
                _player = evt.Player;
                Debug.Log("Local PlayerController connected to Player");
            }
        }

        public void SpawnPlayer ()
        {
            if (!IsHost)
                throw new System.InvalidOperationException("Only host can spawn the player");

            var actorDef = NetworkScriptableObject.Get<ActorDefinition>(_playerClassId.Value);
            if (null == actorDef)
                throw new System.InvalidOperationException($"No ActorDefinition found for player class id [{_playerClassId.Value}]");

            // Make sure the class is unique
            var duplicateClass = GameManager.Instance.Players.Any(p => p.Player != null && p.PlayerClass == PlayerClass);

            // No brain it must be random or an error, pick another random class that isnt one of the other players
            if (duplicateClass || actorDef.Brain == null)
            {
                var defs = GameManager.Instance.ActorDefinitions.Where(d => d.ActorType == ActorType.Player && d.Brain != null && !GameManager.Instance.Players.Any(p => p.PlayerClass == d)).ToArray();
                actorDef = defs[UnityEngine.Random.Range(0, defs.Length)];
                _playerClassId.Value = actorDef.NetworkId;
            }

            // TODO: orientation
            _player = Instantiate(actorDef.Prefab).GetComponent<Player>();
            _player.NetworkObject.SpawnWithOwnership(this.OwnerClientId);
        }

        #region Server RPC

        [ServerRpc] private void SetReadyServerRpc(bool value) => _ready.Value = value;
        [ServerRpc] private void SetPlayerClassIdServerRpc(ushort playerClassId) => _playerClassId.Value = playerClassId;

        #endregion
    }
}
