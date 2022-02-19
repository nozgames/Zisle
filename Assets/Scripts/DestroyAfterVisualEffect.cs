using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace NoZ.Zisle
{
    public class DestroyAfterVisualEffect : MonoBehaviour
    {
        [SerializeField] private VisualEffect _visualEffect = null;

        private void Awake()
        {
            if(null == _visualEffect)
                _visualEffect = GetComponent<VisualEffect>();
        }

        private void OnEnable()
        {
            StartCoroutine(AutoDestroy());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator AutoDestroy()
        {
            // Wait for it to start
            while (!_visualEffect.HasAnySystemAwake())
                yield return null;

            // Wait for it to finishe
            while (_visualEffect.HasAnySystemAwake())
                yield return null;

            VFXManager.Instance.Release(_visualEffect);
        }
    }
}
