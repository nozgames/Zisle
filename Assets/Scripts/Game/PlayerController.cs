using UnityEngine;
using NoZ.Tweening;
using Unity.Netcode;
using NoZ.Animations;
using Unity.Netcode.Components;

namespace NoZ.Zisle
{
    public class PlayerController : NetworkBehaviour
    {
        private enum State
        {
            Idle,
            Run,
            Attack
        }

        [SerializeField] private float _speed = 1.0f;
        [SerializeField] private float _rotationSpeed = 1.0f;
        [SerializeField] private float _moveYaw = 180.0f;
        [SerializeField] private float _cameraYaw = 45.0f;
        [SerializeField] private float _cameraPitch = 45.0f;
        [SerializeField] private float _cameraZoom = 10.0f;
        [SerializeField] private float _cameraZoomMin = 10.0f;
        [SerializeField] private float _cameraZoomMax = 40.0f;
        [SerializeField] private LayerMask _groundLayer = 0;
        [SerializeField] private LayerMask _clipLayer = 0;
        [SerializeField] private LayerMask _attackMask = 0;
        [SerializeField] private float _attackRange = 1.0f;
        [SerializeField] private float _attackRadius= 0.5f;
        [SerializeField] private float _attackCooldown = 0.1f;

        [SerializeField] private SphereCollider _clipCollider = null;
        [SerializeField] private Collider _hitCollider = null;

        [Header("Animations")]
        [SerializeField] private AnimationShader _idleAnimation = null;
        [SerializeField] private AnimationShader _runAnimation = null;
        [SerializeField] private AnimationShader _attackAnimation = null;

        private NetworkVariable<State> _networkState = new NetworkVariable<State>(State.Idle);

        private NetworkObject _networkObject;
        private BlendedAnimationController _animator;
        private State _state;
        private float _cooldown;

        private void Awake()
        {
            _networkObject = GetComponent<NetworkObject>();
            _animator = GetComponent<BlendedAnimationController>();
        }

        [ServerRpc]
        private void SetStateServerRpc (State state)
        {
            _networkState.Value = state;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if(!IsLocalPlayer)
            {
                _networkState.OnValueChanged += (p, n) =>
                {
                    _state = n;

                    System.Collections.IEnumerator Test(Vector3 start)
                    {
                        while (true)
                        {
                            yield return null;

                            if ((transform.position - start).magnitude < 0.001f)
                            {
                                _animator.Play(_idleAnimation);
                                break;
                            }

                            start = transform.position;
                        }
                    }

                    if (n == State.Idle)
                        StartCoroutine(Test(transform.position));
                    else if (n == State.Attack)
                    {
                        //AudioManager.Instance.Play(_attackSwingSound);
                        _animator.Play(_attackAnimation);
                    }
                    else
                        _animator.Play(_runAnimation);
                };
            }

            _state = State.Idle;
            _animator.Play(_idleAnimation);

            if (IsLocalPlayer)
            {
                InputManager.Instance.OnPlayerZoom += (f) => _cameraZoom = Mathf.Clamp(_cameraZoom - 5.0f * f, _cameraZoomMin, _cameraZoomMax);
                InputManager.Instance.OnPlayerAction += () =>
                {
                    if (_state != State.Idle && _state != State.Run)
                        return;

                    if (_cooldown > Time.time)
                        return;

                    //AudioManager.Instance.Play(_attackSwingSound);
                    _animator.Play(_attackAnimation, onComplete: () =>
                    {
                        _hitCollider.enabled = false;
                        if (Physics.SphereCast(transform.position + Vector3.up * 0.2f - transform.forward * _attackRadius, _attackRadius, transform.forward, out var hit, _attackRange + _attackRadius, _attackMask))
                        {
                            var fx = hit.collider.GetComponentInParent<HitEffect>();
                            if (fx != null)
                                AttackHitServerRpc(hit.collider.GetComponentInParent<NetworkObject>().NetworkObjectId);
                        }
                        _hitCollider.enabled = true;

                        _cooldown = Time.time + _attackCooldown;
                        _state = State.Idle;
                        _animator.Play(_idleAnimation);
                        SetStateServerRpc(State.Idle);
                    });
                    _state = State.Attack;
                    SetStateServerRpc(State.Attack);
                };
            }
        }

        [ServerRpc]
        private void AttackHitServerRpc(ulong id)
        {
            if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(id, out var netobj))
                return;

            var fx = netobj.GetComponent<HitEffect>();
            if(fx != null)
                fx.Play();

            //AudioManager.Instance.Play(_attackHitSound);
        }

        private void Update()
        {
            if (!_networkObject.IsLocalPlayer)
                return;


            if(_state == State.Idle || _state == State.Run)
                MoveTo(InputManager.Instance.playerMove * Time.deltaTime * _speed);

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
                moveTarget.y += (hit.point.y - moveTarget.y) * Time.deltaTime * _speed;

            // Set the new position
            transform.position = moveTarget;

            var newState = State.Idle;
            if (moveDelta.magnitude > float.Epsilon)
                newState = State.Run;

            if(newState != _state)
            {
                if (newState == State.Idle)
                    _animator.Play(_idleAnimation);
                else if (newState == State.Run)
                    _animator.Play(_runAnimation);

                SetStateServerRpc(newState);

                _state = newState;
            }


            // If the player is moving then use the original look delta to rotate using the rotation speed
            if(lookDelta.sqrMagnitude > 0.0f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDelta, Vector3.up), Time.deltaTime * _rotationSpeed);
        }
    }
}
