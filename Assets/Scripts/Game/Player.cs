using UnityEngine;
using Unity.Netcode;
using NoZ.Animations;
using System.Collections.Generic;
using UnityEngine.AI;

namespace NoZ.Zisle
{
    public class Player : Actor
    {
        [Header("Player")]
        [SerializeField] private float _rotationSpeed = 1.0f;
        [SerializeField] private float _moveYaw = 180.0f;
        [SerializeField] private float _cameraYaw = 45.0f;
        [SerializeField] private float _cameraPitch = 45.0f;
        [SerializeField] private float _cameraZoom = 10.0f;
        [SerializeField] private float _cameraZoomMin = 10.0f;
        [SerializeField] private float _cameraZoomMax = 40.0f;
        [SerializeField] private LayerMask _groundLayer = 0;
        [SerializeField] private LayerMask _clipLayer = 0;

        [SerializeField] private GameObject _buildThing = null;

        [SerializeField] private SphereCollider _clipCollider = null;

        private float _cooldown;

        public static List<Player> All { get; private set; } = new List<Player> ();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            All.Add(this);

            if (IsLocalPlayer)
            {
                InputManager.Instance.OnPlayerZoom += (f) => _cameraZoom = Mathf.Clamp(_cameraZoom - 5.0f * f, _cameraZoomMin, _cameraZoomMax);
                InputManager.Instance.OnPlayerBuild += (gamepad) =>
                {
                    if (!gamepad)
                    {
                        var look = Vector3.zero;
                        if (Physics.Raycast(InputManager.Instance.PlayerLookRay, out var hit, 100.0f))
                            look = (hit.point - transform.position).ZeroY();
                        else
                            look = (InputManager.Instance.PlayerLook - transform.position).ZeroY();

                        if (look.magnitude >= 0.001f)
                            transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
                    }

                    Instantiate(_buildThing, (transform.position + transform.forward).ZeroY(), transform.rotation * Quaternion.Euler(0,180,0));
                    //IslandManager.Instance.UpdateNavMesh();
                };
                InputManager.Instance.OnPlayerAction += (gamepad) =>
                {
                    if (State != ActorState.Idle && State != ActorState.Run)
                        return;

                    if (_cooldown > Time.time)
                        return;

                    if(!gamepad)
                    {
                        var look = Vector3.zero;
                        if (Physics.Raycast(InputManager.Instance.PlayerLookRay, out var hit, 100.0f) && hit.collider.GetComponentInParent<Actor>() != null)
                            look = (hit.collider.transform.position - transform.position).ZeroY();
                        else
                            look = (InputManager.Instance.PlayerLook - transform.position).ZeroY();

                        if (look.magnitude >= 0.001f)
                            transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
                    }

                    ExecuteAbility(Abilities[0]);
                };
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            All.Remove(this);
        }

        private void Update()
        {
            if (!IsLocalPlayer)
                return;

            if(State == ActorState.Idle || State == ActorState.Run)
                MoveTo(InputManager.Instance.playerMove * Time.deltaTime * GetAttributeValue(ActorAttribute.Speed));

            GameManager.Instance.Camera.transform.position = transform.position + Quaternion.Euler(_cameraPitch, _cameraYaw, 0) * new Vector3(0, 0, 1) * _cameraZoom;
            GameManager.Instance.Camera.transform.LookAt(transform.position, Vector3.up);
        }

        private void MoveTo (Vector2 move)
        {
            var look = Quaternion.Euler(0.0f, _cameraYaw + _moveYaw, 0.0f);
            var lookDelta = look * move.ToVector3XZ();
            var moveDelta = lookDelta;
            var radius = _clipCollider.radius;
            var halfRadius = radius * 0.5f;

            var moveOrigin = _clipCollider.transform.position;
            var moveDistance = moveDelta.magnitude;
            var moveDir = moveDelta.normalized;

            // Sphere cast from half a sphere behind us to ensure we are not starting in the wall
            var ray = new Ray(moveOrigin - moveDir * halfRadius, moveDir);
            if (Physics.SphereCast(ray, radius, out var hit, moveDistance + halfRadius, _clipLayer))
            {
                // Calculate perpendicular to the normal we hit
                var across = Vector3.Cross(hit.normal, Vector3.up);

                // Calculate how much to slide along the perpendicular vector by projecting the amount we overshot
                // down the perpendicular vector 
                var slide = Vector3.Dot(across, (moveOrigin + moveDelta) - hit.point);

                // Recalculate the movement delta to be the point we hit plus the slide
                moveDelta = moveDir * (hit.distance - halfRadius) + slide * across;
                moveDir = moveDelta.normalized;
                moveDistance = moveDelta.magnitude;

                // re-cast the sphere to make sure we can actually move to this new location, if not just go as far as we can
                ray.origin = moveOrigin - moveDir * halfRadius;
                ray.direction = moveDir;
                if (Physics.SphereCast(ray, radius, out hit, moveDistance, _clipLayer))
                    moveDelta = moveDir * (hit.distance - halfRadius);
            }

            // Find the ground
            var moveTarget = transform.position + moveDelta;
            moveTarget.y = transform.position.y;
            if (Physics.Raycast(moveOrigin + Vector3.up * halfRadius, -Vector3.up, out hit, 5.0f, _groundLayer))
            {
                var ychangemax = Time.deltaTime * 0.01f;
                var ychange = Mathf.Clamp(hit.point.y - moveTarget.y, -ychangemax, ychangemax);
                moveTarget.y += ychange;
            }

            // Set the new position
            //GetComponent<NavMeshAgent>().updatePosition = false;
            //GetComponent<NavMeshAgent>().updateRotation = false;
            GetComponent<NavMeshAgent>().Move(moveTarget - transform.position);

            //GetComponent<NavMeshAgent>().SetDestination(moveTarget);
            //transform.position = moveTarget;

            var newState = ActorState.Idle;
            if (moveDelta.magnitude > float.Epsilon)
                newState = ActorState.Run;

            if(newState != State)
                State = newState;

            // If the player is moving then use the original look delta to rotate using the rotation speed
            if(lookDelta.sqrMagnitude > 0.0f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDelta, Vector3.up), Time.deltaTime * _rotationSpeed);
        }
    }
}
