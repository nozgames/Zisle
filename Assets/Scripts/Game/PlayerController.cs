using UnityEngine;
using NoZ.Tweening;
using Unity.Netcode;

namespace NoZ.Zisle
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float _speed = 1.0f;
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
                    new Vector3(0,_moveBounceHeight,0)).Duration(0.2f).PingPong())
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
            var move3d = look * move.ToVector3XZ();

            if (!Physics.Raycast(transform.position + move3d + Vector3.up, -Vector3.up, out var hit, 5.0f, _groundLayer))
                return;

            transform.position += move3d;

            if (move3d.sqrMagnitude > 0)
            {
                transform.TweenRotation(Quaternion.LookRotation(move3d)).Duration(_rotateDuration).Play();
                _moveTween.Update(Time.deltaTime);
            }
        }
    }
}
