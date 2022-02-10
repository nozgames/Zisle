using NoZ.Animations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public class PlayAnimationOnEnable : MonoBehaviour
    {
        [SerializeField] AnimationShader _clip = null;
        [SerializeField] AnimationShader _idle = null;

        private void OnEnable()
        {
            Play(_clip);

            if (!_clip.isLooping)
                StartCoroutine(Repeat());
        }

        private IEnumerator Repeat ()
        {
            while (isActiveAndEnabled)
            {
                yield return new WaitForSeconds(_clip.length * 1.0f / _clip.speed);

                if (_idle != null)
                {
                    Play(_idle);
                    yield return new WaitForSeconds(0.2f);
                }

                Play(_clip);
            }
        }

        private void Play(AnimationShader shader)
        {
            var a = GetComponent<BlendedAnimationController>();
            if (a != null)
                a.Play(shader);
        }
    }
}
