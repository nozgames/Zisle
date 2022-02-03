using UnityEngine;
using NoZ.Tweening;
using Unity.Netcode;

namespace NoZ.Zisle
{
    public class DestroyAfterSeconds : MonoBehaviour
    {
        [SerializeField] private float _seconds = 1.0f;

        private NetworkObject _netobj;

        private void Awake()
        {
            _netobj = GetComponent<NetworkObject>();
        }

        private void OnEnable()
        {
            if (_netobj != null && !NetworkManager.Singleton.IsHost)
                return;

            gameObject.TweenWait(_seconds).DestroyOnStop().Play();
        }
    }
}
