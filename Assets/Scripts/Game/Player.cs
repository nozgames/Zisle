using UnityEngine;
using System.Collections.Generic;
using NoZ.Events;
using System.Linq;
using NoZ.Zisle.UI;

namespace NoZ.Zisle
{
    public enum PlayerButton
    {
        Action
    }

    public class Player : Actor
    {
        private static readonly int PlayerButtonCount = System.Enum.GetNames(typeof(PlayerButton)).Length;

        [Header("Player")]
        [SerializeField] private float _rotationSpeed = 1.0f;
        [SerializeField] private float _buttonSlop = 0.4f;

        private struct PlayerButtonState
        {
            /// <summary>
            /// Time the player button was last pressed
            /// </summary>
            public float LastPressedTime;

            /// <summary>
            /// True if the last time the action was pressed it was with a gamepad
            /// </summary>
            public bool LastPressedGamepad;
        }

        private PlayerButton _lastPressed = PlayerButton.Action;
        private PlayerButtonState[] _buttonStates = new PlayerButtonState[PlayerButtonCount];
        
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
                InputManager.Instance.OnPlayerAction += OnPlayerAction;
            }

            GameEvent.Raise(this, new PlayerSpawned { Player = this });
        }

        private void OnPlayerZoom (float f) => GameManager.Instance.CameraZoom -= 3.0f * f;
        
        private void OnPlayerAction (bool gamepad)
        {
            // Snap look
            var look = Vector3.zero;
            if (gamepad)
            {
                look = InputManager.Instance.PlayerMove;
            }
            else
            {
                if (Physics.Raycast(InputManager.Instance.PlayerLookRay, out var hit, 100.0f) && hit.collider.GetComponentInParent<Actor>() != null)
                    look = (hit.collider.transform.position - transform.position);
                else
                    look = (InputManager.Instance.PlayerLook - transform.position);
            }

            look = look.ZeroY();
            if (look.magnitude >= 0.001f)
                transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);

            SetLastPressed(PlayerButton.Action, gamepad);
        }

        private void SetLastPressed (PlayerButton button, bool gamepad)
        {
            _lastPressed = button;

            ref var state = ref _buttonStates[(int)button];
            state.LastPressedTime = Time.time;
            state.LastPressedGamepad = gamepad;
        }

        private void ClearLastPressed ()
        {
            ref var state = ref _buttonStates[(int)_lastPressed];
            state.LastPressedTime = 0.0f;
            state.LastPressedGamepad = false;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            GameEvent.Raise(this, new PlayerDespawned { Player = this });

            InputManager.Instance.OnPlayerZoom -= OnPlayerZoom;
            InputManager.Instance.OnPlayerAction -= OnPlayerAction;
        }

        protected override void Update()
        {
            base.Update();

            if (!IsOwner)
                return;

            if (!NavAgent.enabled && GameManager.Instance.Game.HasIslands)
                NavAgent.enabled = true;

            var moveSpeed = 1.0f;
            if (!NavAgent.isOnNavMesh)
                moveSpeed = 0.0f;
            else if (IsBusy && LastAbilityUsed != null && LastAbilityUsed.MoveSpeed > 0.0f)
                moveSpeed = LastAbilityUsed.MoveSpeed;
            else if (IsBusy)
                moveSpeed = 0.0f;

            var y = transform.position.y;
            if(moveSpeed > 0.0f)
                MoveTo(InputManager.Instance.PlayerMove * Time.deltaTime * GetAttributeValue(ActorAttribute.Speed) * moveSpeed);

            //transform.position = new Vector3(transform.position.x, y, transform.position.z);
            SnapToGround();

            GameManager.Instance.FrameCamera(transform.position);
        }

        private void MoveTo (Vector3 offset)
        {
            var moveTarget = (transform.position + offset).ZeroY();

            // Set the new position
            if(NavAgent.enabled)
                NavAgent.Move(moveTarget - transform.position);

            // If the player is moving then use the original look delta to rotate using the rotation speed
            if(IsMoving && offset.magnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(offset, Vector3.up), Time.deltaTime * _rotationSpeed);
        }

        public override bool ExecuteAbility(ActorAbility ability, List<Actor> targets)
        {
            ClearLastPressed();
            return base.ExecuteAbility(ability, targets);
        }

        /// <summary>
        /// Returns true if the given player button was pressed
        /// </summary>
        public bool WasButtonPressed (PlayerButton button) =>
            !IsBusy && _lastPressed == button && (Time.time - _buttonStates[(int)button].LastPressedTime) < _buttonSlop;
    }
}
