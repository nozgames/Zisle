using UnityEngine;
using System.Collections.Generic;
using NoZ.Events;
using System.Linq;
using NoZ.Zisle.UI;
using Unity.Netcode;

namespace NoZ.Zisle
{
    public enum PlayerButton
    {
        Primary,
        Secondary
    }

    public class Player : Actor
    {
        private static readonly int PlayerButtonCount = System.Enum.GetNames(typeof(PlayerButton)).Length;

        [Header("Player")]
        [SerializeField] private float _rotationSpeed = 1.0f;
        [SerializeField] private float _buttonSlop = 0.4f;
        [SerializeField] private float _buttonRepeat = 0.1f;

        private PlayerButton _lastPressed = PlayerButton.Primary;
        
        public PlayerController Controller { get; private set; }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            name = $"Player{OwnerClientId}";

            Controller = GameManager.Instance.Players.Where(p => p.OwnerClientId == OwnerClientId).FirstOrDefault();

            NavAgent.updateRotation = false;

            if (IsOwner)
            {
                InputManager.Instance.OnPlayerZoom += OnPlayerZoom;
                InputManager.Instance.OnPlayerButton += OnButtonPress;
            }

            GameEvent.Raise(this, new PlayerSpawned { Player = this });
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            GameEvent.Raise(this, new PlayerDespawned { Player = this });

            if (IsOwner)
            {
                InputManager.Instance.OnPlayerZoom -= OnPlayerZoom;
                InputManager.Instance.OnPlayerButton -= OnButtonPress;
            }
        }

        private void OnPlayerZoom (float f) => CameraManager.Instance.IsometricZoom -= 3.0f * f;

        private void OnButtonPress (PlayerButton button)
        {
            var look = InputManager.Instance.PlayerLook;
            var destination = new Destination(look);

            if(Physics.Raycast(InputManager.Instance.PlayerLookRay, out var hit, 100.0f, -1))
            {
                var actor = hit.collider.GetComponentInParent<Actor>();
                if (actor != null)
                    destination = new Destination(actor);
            }

            OnButtonPress(button, destination);
        }

        private void OnButtonPress (PlayerButton button, Destination destination)
        {
            if (IsHost)
            {
                _lastPressed = button;
                SetDestination(destination);
            }
            else
                SetDestinationServerRpc(button, destination);
        }

        [ServerRpc]
        private void SetDestinationServerRpc(PlayerButton button, Destination destination) =>
            OnButtonPress(button, destination);

        protected override void Update()
        {
            base.Update();

            if (!IsOwner || State != ActorState.Active)
                return;

            GameManager.Instance.ListenAt(transform);

            CameraManager.Instance.IsometricTarget = transform.position;
        }

        private void MoveTo (Vector3 offset)
        {
            var moveTarget = (transform.position + offset).ZeroY();

            // Set the new position
            if(NavAgent.enabled)
                NavAgent.Move(moveTarget - transform.position);

            // If the player is moving then use the original look delta to rotate using the rotation speed
            if(!IsBusy && IsMoving && offset.magnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(offset, Vector3.up), Time.deltaTime * _rotationSpeed);
        }

        /// <summary>
        /// Returns true if the given player button was pressed
        /// </summary>
        public bool WasButtonPressed (PlayerButton button) => !IsBusy && _lastPressed == button;

        protected override void OnHealthChanged()
        {
            base.OnHealthChanged();

            Health = Mathf.Max(Health, 1.0f);
        }
    }
}
