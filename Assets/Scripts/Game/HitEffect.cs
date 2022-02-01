using UnityEngine;
using NoZ.Tweening;
using Unity.Netcode;

namespace NoZ.Zisle
{
    public class HitEffect : NetworkBehaviour
    {
        [SerializeField] private Renderer _renderer = null;
        [SerializeField] private float _duration = 0.5f;
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private Vector3 _scale = Vector3.one;
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

            if (_scale != Vector3.one)
            {
                _renderer.transform.localScale = Vector3.one;
                _tween.Element(_renderer.transform.TweenLocalScale(_scale).Duration(_duration).EaseOutCubic().PingPong());
            }

            if(_rotate != Vector3.one)
            {
                _renderer.transform.localRotation = Quaternion.identity;
                _tween.Element(_renderer.transform.TweenLocalRotation(Quaternion.Euler(_rotate)).Duration(_duration).EaseOutCubic().PingPong());
            }

            _tween.Play();
        }
    }
}
