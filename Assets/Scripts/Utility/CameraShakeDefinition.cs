using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Camera Shake")]
    public class CameraShakeDefinition : ScriptableObject
    {
        [SerializeField] private float _intensity = 1.0f;
        [SerializeField] private float _duration = 1.0f;

        public void Shake (float intensity=1.0f)
        {
            GameManager.Instance.ShakeCamera(_intensity * intensity, _duration);
        }
    }
}
