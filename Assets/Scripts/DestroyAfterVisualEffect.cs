using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace NoZ.Zisle
{
    public class DestroyAfterVisualEffect : MonoBehaviour
    {
        [SerializeField] private VisualEffect _visualEffect = null;

        private void OnEnable()
        {
            _visualEffect.Play();
            _visualEffect.AdvanceOneFrame();

            StartCoroutine(AutoDestroy());
        }

        private IEnumerator AutoDestroy()
        {
            yield return new WaitForSeconds(0.1f);

            while (_visualEffect.HasAnySystemAwake())
                yield return null;

            Destroy(gameObject);
        }
    }
}
