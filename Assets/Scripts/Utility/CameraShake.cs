using UnityEngine;
using NoZ.Tweening;

namespace NoZ.Zisle
{
    public class CameraShake : MonoBehaviour
    {
        private class ShakeProvider : Vector3Provider<CameraShake>
        {
            protected override Vector3 GetValue(CameraShake target) => target.ShakeOffset;
            protected override void SetValue(CameraShake target, Vector3 value) => target.ShakeOffset = value;
            protected override Vector3 Evalulate(Vector3 from, Vector3 to, float normalizedTime, Vector3Options options) =>
                new Vector3(
                    to.x * (Mathf.PerlinNoise(100.0f, normalizedTime * 20f) - 0.5f) * 2.0f,
                    to.y * (Mathf.PerlinNoise(100.0f, normalizedTime * 20f) - 0.5f) * 2.0f,
                    to.z * (Mathf.PerlinNoise(100.0f, normalizedTime * 20f) - 0.5f) * 2.0f
                    ) * (1f - normalizedTime);
        }

        private static ShakeProvider _shakeProvider = new ShakeProvider();
        private Vector3 _shakeOffset;

        public Vector3 PositionOffset { get; private set; }
        public Vector3 RotationOffset { get; private set; }

        private Vector3 ShakeOffset
        {
            get => _shakeOffset;
            set
            {
                _shakeOffset = value;
                PositionOffset = _shakeOffset;
                RotationOffset = _shakeOffset;
            }
        }

        public void Shake (float intensity, float duration)
        {
            Tween.To(_shakeProvider, this, Vector3.one * intensity).Duration(duration).Play();
        }
    }
}
