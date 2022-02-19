using UnityEngine;
using Cinemachine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Manages the cameras within the game
    /// </summary>
    public class CameraManager : Singleton<CameraManager>
    {
        [SerializeField] private Camera _camera = null;
        [SerializeField] private CinemachineBrain _brain = null;
        [SerializeField] private CameraShake _cameraShake = null;

        [Header("Isometric")]
        [SerializeField] private CinemachineVirtualCamera _isometricCamera = null;
        [SerializeField] private Transform _isometricTarget = null;
        [SerializeField] private float _isometricYaw = 45.0f;
        [SerializeField] private float _isometricPitch = 45.0f;
        [SerializeField] private float _isometricZoomMin = 10.0f;
        [SerializeField] private float _isometricZoomMax = 40.0f;

        [Header("Cinematic")]
        [SerializeField] private CinemachineVirtualCamera _cinematicCamera = null;
        [SerializeField] private Transform _cinematicTarget = null;

        private float _isometricZoom = 20.0f;

        /// <summary>
        /// Returns the primary camera
        /// </summary>
        public Camera Camera => _camera;

        /// <summary>
        /// Returns the yaw value used for the isometric view
        /// </summary>
        public float IsometricYaw => _isometricYaw;

        /// <summary>
        /// Position of isometric target
        /// </summary>
        public Vector3 IsometricTarget
        {
            get => _isometricTarget.position;
            set
            {
                _isometricTarget.position = value.ZeroY();
                UpdateIsometricCamera();
            }
        }

        /// <summary>
        /// Current isometric zoom
        /// </summary>
        public float IsometricZoom
        {
            get => _isometricZoom;
            set
            {
                _isometricZoom = Mathf.Clamp(value, _isometricZoomMin, _isometricZoomMax);
                UpdateIsometricCamera();
            }
        }

        /// <summary>
        /// Current isometric zoom as a ratio of 0-1 from min-max
        /// </summary>
        public float IsometricZoomRatio => (_isometricZoom - _isometricZoomMin) / (_isometricZoomMax - _isometricZoomMin);

        /// <summary>
        /// Distance of the isometric camera to the target
        /// </summary>
        public float IsometricDistance { get; private set; }

        /// <summary>
        /// Start a cinematic sequence by transitioning to the cinematic camera.
        /// </summary>
        public void StartCinematic (Vector3 target, float zoom)
        {
            // TODO: if far away we should probably cross fade to the camera so its not so jarring

            _cinematicTarget.transform.position = target.ZeroY();
            _cinematicCamera.enabled = true;
            _cinematicCamera.transform.position =
                target.ZeroY() +
                Quaternion.Euler(_isometricPitch, _isometricYaw, 0) * new Vector3(0, 0, 1) * zoom;
        }

        /// <summary>
        /// Stop a cinematic in progress by transitioning back to the isometric camera
        /// </summary>
        public void StopCinematic ()
        {
            _cinematicCamera.enabled = false;
        }

        /// <summary>
        /// Update the isometric camera after one of the parameters changed
        /// </summary>
        private void UpdateIsometricCamera()
        {
            _isometricCamera.transform.position = 
                IsometricTarget + 
                Quaternion.Euler(_isometricPitch, _isometricYaw, 0) * new Vector3(0, 0, 1) * _isometricZoom;
            IsometricDistance = (_isometricCamera.transform.position - IsometricTarget).magnitude;
        }
    }
}
