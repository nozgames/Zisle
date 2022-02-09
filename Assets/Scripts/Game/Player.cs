using UnityEngine;
using System.Collections.Generic;
using NoZ.Events;

namespace NoZ.Zisle
{
    public class Player : Actor
    {
        [Header("Player")]
        [SerializeField] private float _rotationSpeed = 1.0f;
        [SerializeField] private float _actionSlop = 0.2f;

        private float _lastActionTime = 0.0f;
        private bool _lastActionGamepad = false;

        public static List<Player> All { get; private set; } = new List<Player> ();

        public PlayerController Controller { get; set; }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            name = $"Player{OwnerClientId}";

            All.Add(this);

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
            _lastActionGamepad = gamepad;
            _lastActionTime = Time.time;

            if (IsBusy)
                return;

            ExecuteAction(gamepad);
        }

        private void ExecuteAction (bool gamepad)
        {
            _lastActionTime = 0.0f;
            _lastActionGamepad = false;

            // Snap look
            var look = Vector3.zero;
            if (gamepad)
            {
                look = InputManager.Instance.PlayerMove;
            }
            else if (gamepad)
            {
                if (Physics.Raycast(InputManager.Instance.PlayerLookRay, out var hit, 100.0f) && hit.collider.GetComponentInParent<Actor>() != null)
                    look = (hit.collider.transform.position - transform.position);
                else
                    look = (InputManager.Instance.PlayerLook - transform.position);
            }

            look = look.ZeroY();
            if (look.magnitude >= 0.001f)
                transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);

            // Use best ability
            foreach (var ability in Abilities)
                if (ExecuteAbility(ability))
                    break;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            GameEvent.Raise(this, new PlayerDespawned { Player = this });

            InputManager.Instance.OnPlayerZoom -= OnPlayerZoom;
            InputManager.Instance.OnPlayerAction -= OnPlayerAction;

            All.Remove(this);
        }

        protected override void Update()
        {
            base.Update();

            if (!IsOwner)
                return;

            if (!NavAgent.enabled && GameManager.Instance.Game.HasIslands)
                NavAgent.enabled = true;

            if(!IsBusy)
                MoveTo(InputManager.Instance.PlayerMove * Time.deltaTime * GetAttributeValue(ActorAttribute.Speed));

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

        protected override void OnBusyChanged() 
        {
            base.OnBusyChanged();

            if (!IsBusy && (Time.time - _lastActionTime) < _actionSlop)
                ExecuteAction(_lastActionGamepad);
        }
    }
}
