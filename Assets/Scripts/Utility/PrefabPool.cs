using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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

        public bool IsNetworkObject { get; private set; }

        private void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

            IsNetworkObject = _prefab != null && _prefab.GetComponent<NetworkObject>() != null;
        }

#if UNITY_EDITOR
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

            GameObject go = null;
            if(_poolTransform.childCount > 0)
            {
                go = _poolTransform.GetChild(_poolTransform.childCount-1).gameObject;
                go.transform.SetParent(parent);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
            else
            {
                go = Instantiate(_prefab, parent);
                go.AddComponent<PooledPrefab>().Pool = this;
                go.name = _prefab.name;
            }

            if (IsNetworkObject)
                go.GetComponent<NetworkObject>().Spawn(true);

            return go;
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
