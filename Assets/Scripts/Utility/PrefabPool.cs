using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Prefab Pool")]
    public class PrefabPool : ScriptableObject
    {
        private class PooledPrefab : MonoBehaviour
        {
            public PrefabPool Pool { get; set; }
        }

        [SerializeField] private GameObject _prefab = null;

        private Transform _poolTransform = null;

#if UNITY_EDITOR
        private void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if(change == PlayModeStateChange.ExitingPlayMode && _poolTransform != null)
            {
                Destroy(_poolTransform.gameObject);
                _poolTransform = null;
            }
        }

#endif

        public GameObject Instantiate (Transform parent)
        {
            if(_poolTransform == null)
            {
                var poolGameObject = new GameObject();
                poolGameObject.name = name;
                //poolGameObject.hideFlags = HideFlags.HideAndDontSave;
                poolGameObject.SetActive(false);
                DontDestroyOnLoad(poolGameObject);
                _poolTransform = poolGameObject.transform;
            }

            if(_poolTransform.childCount > 0)
            {
                var go = _poolTransform.GetChild(_poolTransform.childCount-1).gameObject;
                go.transform.SetParent(parent);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                return go;
            }
            else
            {
                var go = Instantiate(_prefab, parent);
                go.AddComponent<PooledPrefab>().Pool = this;
                go.name = _prefab.name;
                return go;
            }
        }

        public static void Destroy (GameObject go)
        {
            if (!go.TryGetComponent<PooledPrefab>(out var pooledPrefab))
            {
                Destroy((Object)go);
                return;
            }
            
            var pool = pooledPrefab.Pool;
            go.transform.SetParent(pool._poolTransform);
        }
    }

    public static class PrefabPoolExtensions
    {
        public static void PooledDestroy(this GameObject go) => PrefabPool.Destroy(go);
    }
}
