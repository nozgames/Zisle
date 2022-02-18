using UnityEngine;
using NoZ.Tweening;
using Unity.Netcode;

namespace NoZ.Zisle
{
    public class HitEffect : NetworkBehaviour
    {
        [Header("General")]
        [SerializeField] private float _duration = 0.5f;

        [Header("Color")]
        [SerializeField] private Renderer[] _renderers = null;
        [SerializeField] private Color _color = Color.white;

        [Header("Scale")]
        [SerializeField] private Transform _scaleTransform = null;
        [SerializeField] private Vector3 _scale = Vector3.one;

        [Header("Rotate")]
        [SerializeField] private Transform _rotateTransform = null;
        [SerializeField] private Vector3 _rotate = Vector3.zero;

        private Tween _tween;

        public void Play(Color color, float strength)
        {
            _tween.Stop();
            _tween = this.TweenGroup();

            if (_renderers != null && color != Color.clear)
            {
                foreach (var renderer in _renderers)
                {
                    renderer.material.SetColor(ShaderPropertyID.HitColor, Color.clear);
                    _tween.Element(renderer.material.TweenColor(ShaderPropertyID.HitColor, color).Duration(_duration).EaseOutCubic().PingPong());
                }
            }

            if (_scaleTransform != null)
            {
                _scaleTransform.localScale = Vector3.one;
                _tween.Element(_scaleTransform.TweenLocalScale(_scale * strength).Duration(_duration).EaseOutCubic().PingPong());
            }

            if(_rotateTransform)
            {
                _rotateTransform.localRotation = Quaternion.identity;
                _tween.Element(_rotateTransform.TweenLocalRotation(Quaternion.Euler(_rotate * strength)).Duration(_duration).EaseOutCubic().PingPong());
            }

            _tween.Play();
        }

        private void OnDisable()
        {
            _tween.Stop(false);
        }
    }
}
