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
        private NetworkVariable<FixedString64Bytes> _playerName = new NetworkVariable<FixedString64Bytes>();
        private NetworkVariable<Color> _playerSkinColor = new NetworkVariable<Color>();
        private NetworkVariable<ulong> _playerClassId = new NetworkVariable<ulong>();
        private NetworkVariable<bool> _ready = new NetworkVariable<bool>();

        private Player _player = null;

        /// <summary>
        /// True if the player is ready to start the game
        /// </summary>
        public bool IsReady
        {
            get => _ready.Value;
            set
            {
                if (!IsLocalPlayer || _ready.Value == value)
                    return;

                SetReadyServerRpc(value);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            GameEvent<PlayerSpawned>.OnRaised += OnPlayerSpawned;
            GameEvent<PlayerDespawned>.OnRaised += OnPlayerDespawned;

            GameEvent.Raise(this, new PlayerConnected { PlayerController = this});

            // Tell anyone who cares that our name changed
            _playerName.OnValueChanged += (p, n) => GameEvent.Raise(this, new PlayerNameChanged { PlayerController = this });
            _ready.OnValueChanged += (p, n) => GameEvent.Raise(this, new PlayerReadyChanged { PlayerController = this });

            // Send the player options
            if (IsLocalPlayer)
                SetOptionsServerRpc(Options.PlayerName, GameManager.Instance.ActorDefinitions.Where(d => d.ActorType == ActorType.Player).FirstOrDefault().NetworkId, Color.white);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            GameEvent<PlayerSpawned>.OnRaised -= OnPlayerSpawned;
            GameEvent<PlayerDespawned>.OnRaised -= OnPlayerDespawned;

            GameEvent.Raise(this, new PlayerDisconnected { PlayerController = this });
        }

        private void OnPlayerDespawned(object sender, PlayerDespawned evt)
        {
            if (evt.Player.OwnerClientId == this.OwnerClientId)
            {
                _player = evt.Player;
                Debug.Log("Local PlayerController connected to Player");
            }
        }

        private void OnPlayerSpawned(object sender, PlayerSpawned evt)
        {
            if (evt.Player.OwnerClientId == this.OwnerClientId)
            {
                _player = null;
                Debug.Log("Local PlayerController disconnected from Player");
            }
        }

        public void SpawnPlayer ()
        {
            if (!IsHost)
                throw new System.InvalidOperationException("Only host can spawn the player");

            var actorDef = NetworkScriptableObject.Get<ActorDefinition>(_playerClassId.Value);
            if (null == actorDef)
                throw new System.InvalidOperationException($"No ActorDefinition found for player class id [{_playerClassId.Value}]");

            // TODO: orientation
            _player = Instantiate(actorDef.Prefab).GetComponent<Player>();
            _player.Controller = this;
            _player.NetworkObject.SpawnWithOwnership(this.OwnerClientId);
        }

        #region Server RPC
        
        [ServerRpc]
        private void SetOptionsServerRpc (string name, ulong playerClassId, Color skinColor)
        {
            _playerName.Value = name;
            _playerClassId.Value = playerClassId;
            _playerSkinColor.Value = skinColor;
        }

        [ServerRpc] private void SetReadyServerRpc(bool value) => _ready.Value = value;
        
        #endregion
    }
}
