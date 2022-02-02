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
        [SerializeField] private Renderer _renderer = null;
        [SerializeField] private Color _color = Color.white;

        [Header("Scale")]
        [SerializeField] private Transform _scaleTransform = null;
        [SerializeField] private Vector3 _scale = Vector3.one;

        [Header("Rotate")]
        [SerializeField] private Transform _rotateTransform = null;
        [SerializeField] private Vector3 _rotate = Vector3.zero;

        private Tween _tween;

        public void Play()
        {
            PlayClientRpc();
        }

        [ClientRpc]
        private void PlayClientRpc()
        {
            if (_renderer == null)
                return;

            _tween.Stop();
            _tween = this.TweenGroup();

            if (_color != Color.clear)
            {
                _renderer.material.SetColor(ShaderPropertyID.HitColor, Color.clear);
                _tween.Element(_renderer.material.TweenColor(ShaderPropertyID.HitColor, _color).Duration(_duration).EaseOutCubic().PingPong());
            }

            if (_scaleTransform != null)
            {
                _scaleTransform.localScale = Vector3.one;
                _tween.Element(_scaleTransform.TweenLocalScale(_scale).Duration(_duration).EaseOutCubic().PingPong());
            }

            if(_rotateTransform)
            {
                _rotateTransform.localRotation = Quaternion.identity;
                _tween.Element(_rotateTransform.TweenLocalRotation(Quaternion.Euler(_rotate)).Duration(_duration).EaseOutCubic().PingPong());
            }

            _tween.Play();
        }
    }
}
