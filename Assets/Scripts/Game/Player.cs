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
        None,
        Primary,
        Secondary
    }

    public class Player : Actor
    {
        private static readonly int PlayerButtonCount = System.Enum.GetNames(typeof(PlayerButton)).Length;

        [Header("Player")]
        [SerializeField] private float _buttonRepeat = 0.25f;

        private PlayerButton _pendingButton;
        private Destination _pendingDestination;
        private PlayerButton _button = PlayerButton.Primary;
        private bool _buttonPressed = false;
        private double _buttonRepeatTime = 0;
        
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
                InputManager.Instance.OnPlayerButtonDown += OnButtonDown;
                InputManager.Instance.OnPlayerButtonUp += OnButtonUp;
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
                InputManager.Instance.OnPlayerButtonDown -= OnButtonDown;
                InputManager.Instance.OnPlayerButtonUp -= OnButtonUp;
            }
        }

        private void OnPlayerZoom (float f) => CameraManager.Instance.IsometricZoom -= 3.0f * f;

        private void OnButtonDown (PlayerButton button)
        {
            var look = InputManager.Instance.PlayerLook;
            var destination = new Destination(look);

            if (Physics.Raycast(InputManager.Instance.PlayerLookRay, out var hit, 100.0f, -1))
            {
                var actor = hit.collider.GetComponentInParent<Actor>();
                if (actor != null)
                    destination = new Destination(actor);
            }

            if(destination.Target == null)
                button = PlayerButton.None;

            OnButtonDown(button, destination);
        }

        private void OnButtonUp (PlayerButton button)
        {
            _buttonPressed = false;

            if (!IsHost)
                OnButtonUpServerRpc(button);
        }

        private void OnButtonDown (PlayerButton button, Destination destination)
        {
            if(!IsHost)
            {
                _buttonRepeatTime = Time.timeAsDouble + _buttonRepeat;
                _buttonPressed = true;
                _button = button;

                OnButtonDownServerRpc(button, destination);
            }
            else
            {
                if (IsBusy)
                {
                    _pendingButton = _button;
                    _pendingDestination = destination;
                }
                else
                {
                    _buttonRepeatTime = Time.timeAsDouble + _buttonRepeat;
                    _buttonPressed = true;
                    _button = button;
                    SetDestination(destination);
                }
            }   
        }

        [ServerRpc]
        private void OnButtonDownServerRpc (PlayerButton button, Destination destination) =>
            OnButtonDown(button, destination);

        [ServerRpc]
        private void OnButtonUpServerRpc (PlayerButton button) =>
            OnButtonUp(button);

        protected override void OnAbilityEnd()
        {
            base.OnAbilityEnd();

            if (IsHost && _pendingDestination.IsValid)
            {
                OnButtonDown(_pendingButton, _pendingDestination);
                _pendingDestination = Destination.None;
                _pendingButton = PlayerButton.None;
            }

            if (IsHost && !_buttonPressed)
            {
                _button = PlayerButton.None;
                SetDestination(Destination.None);
            }
        }

        protected override void Update()
        {
            base.Update();

            // If move button is held down then update the destination every repeat
            if (IsOwner && _buttonPressed && Destination.IsValid && !Destination.HasTarget && Time.timeAsDouble >= _buttonRepeatTime)
                OnButtonDown(PlayerButton.None, new Destination(InputManager.Instance.PlayerLook));
            else if (IsHost && !IsBusy && _buttonPressed && Time.timeAsDouble >= _buttonRepeatTime)
                OnButtonDown(_button, Destination);

            if (!IsOwner || State != ActorState.Active)
                return;

            GameManager.Instance.ListenAt(transform);

            CameraManager.Instance.IsometricTarget = transform.position;
        }

        /// <summary>
        /// Returns true if the given player button was pressed
        /// </summary>
        public bool WasButtonPressed (PlayerButton button) => !IsBusy && _button == button;

        protected override void OnHealthChanged()
        {
            base.OnHealthChanged();

            Health = Mathf.Max(Health, 1.0f);
        }
    }
}
