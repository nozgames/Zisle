using UnityEngine;
using NoZ.Tweening;
using Unity.Netcode;

namespace NoZ.Zisle
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float _speed = 1.0f;
        [SerializeField] private float _rotationSpeed = 1.0f;
        [SerializeField] private float _rotateDuration = 0.05f;
        [SerializeField] private float _moveBounceHeight = 0.5f;
        [SerializeField] private Transform _test = null;
        [SerializeField] private float _moveYaw = 180.0f;
        [SerializeField] private float _cameraYaw = 45.0f;
        [SerializeField] private float _cameraPitch = 45.0f;
        [SerializeField] private float _cameraZoom = 10.0f;
        [SerializeField] private float _cameraZoomMin = 10.0f;
        [SerializeField] private float _cameraZoomMax = 40.0f;
        [SerializeField] private LayerMask _groundLayer = 0;
        [SerializeField] private LayerMask _clipLayer = 0;

        [SerializeField] private SphereCollider _clipCollider = null;

        Tween _moveTween;

        private NetworkObject _networkObject;

        private void Awake()
        {
            _networkObject = GetComponent<NetworkObject>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _moveTween = this.TweenGroup()
                .Element(Tween.FromTo(
                    QuaternionMemberProvider<Transform>.Get("localRotation"),
                    _test,
                    Quaternion.Euler(-5, 90, 0),
                    Quaternion.Euler(5, 90, 0))
                    .Duration(0.2f)
                    .PingPong()
                ).Element(Tween.FromTo(
                    Vector3MemberProvider<Transform>.Get("localPosition"),
                    _test,
                    new Vector3(0,0, 0),
                    new Vector3(0,0,0)).Duration(0.2f).PingPong())
                .UpdateMode(UpdateMode.Manual)
                .Loop()
                .Play();

            if (IsLocalPlayer)
            {
                InputManager.Instance.OnPlayerZoom += (f) => _cameraZoom = Mathf.Clamp(_cameraZoom - 5.0f * f, _cameraZoomMin, _cameraZoomMax);
            }
        }

        private void Update()
        {
            if (!_networkObject.IsLocalPlayer)
                return;

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

            // Keep the animation going if we are actually moving
            if(moveDelta.sqrMagnitude > 0.0f)
                _moveTween.Update(Time.deltaTime);

            // If the player is moving then use the original look delta to rotate using the rotation speed
            if(lookDelta.sqrMagnitude > 0.0f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDelta, Vector3.up), Time.deltaTime * _rotationSpeed);
        }
    }
}
