using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.VFX;

namespace NoZ.Zisle
{
    public class VFXManager : Singleton<VFXManager>
    {
        [SerializeField] private int _maxPoolSize = 128;

        private LinkedPool<VisualEffect> _pool;

        private List<VisualEffect> _playing = new List<VisualEffect>();

        public override void Initialize()
        {
            base.Initialize();

            _pool = new LinkedPool<VisualEffect>(
                        createFunc: PoolCreate,
                        actionOnGet: PoolGet,
                        actionOnRelease: PoolRelease,
                        actionOnDestroy: PoolDestroy,
                        maxSize: _maxPoolSize
                        );
        }

        public override void Shutdown()
        {
            base.Shutdown();

            _pool.Clear();
        }

        private void PoolDestroy(VisualEffect ve)
        {
            Destroy(ve.gameObject);
        }

        private void PoolRelease(VisualEffect ve)
        {
            //ve.gameObject.SetActive(false);
            ve.visualEffectAsset = null;
            ve.transform.SetParent(transform);
        }

        private void PoolGet(VisualEffect ve)
        {
        }

        private VisualEffect PoolCreate()
        {
            var go = new GameObject();
            var ve = go.AddComponent<VisualEffect>();
            ve.initialEventName = "";
            //go.AddComponent<DestroyAfterVisualEffect>();
            go.transform.SetParent(transform);
            go.name = "VisualEffect";
            return ve;
        }

        /// <summary>
        /// Play a VisualEffect and attach it to the given transform
        /// </summary>
        public VisualEffect Play(VisualEffectAsset asset, Transform transform)
        {
            var ve = _pool.Get();
            //ve.gameObject.SetActive(true);
            ve.transform.SetParent(transform);
            ve.transform.localPosition = Vector3.zero;
            ve.transform.localRotation = Quaternion.identity;
            ve.visualEffectAsset = asset;
            ve.Play();
            return ve;
        }

        /// <summary>
        /// Play a VisualEffect at the given <paramref name="position"/> and <paramref name="rotation"/>
        /// </summary>
        public VisualEffect Play(VisualEffectAsset asset, Vector3 position, Quaternion rotation)
        {
            var ve = _pool.Get();
            ve.transform.position = position;
            ve.transform.rotation = rotation;
            ve.visualEffectAsset = asset;
            ve.Play();

            _playing.Add(ve);
            return ve;
        }

        /// <summary>
        /// Release an effect back to the pool
        /// </summary>
        public void Release (VisualEffect ve)
        {
            ve.Stop();

            if (!ve.HasAnySystemAwake())
                _pool.Release(ve);
        }

        private void LateUpdate()
        {
            for (int i = _playing.Count - 1; i >= 0; i--)
            {
                if (!_playing[i].HasAnySystemAwake())
                {
                    _pool.Release(_playing[i]);
                    _playing.RemoveAt(i);
                }
            }
        }
    }
}
