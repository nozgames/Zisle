using UnityEngine;
using NoZ.Tweening;
using Unity.Netcode;

namespace NoZ.Zisle
{
    public class DestroyAfterAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        private NetworkObject _netobj;

        private void Awake()
        {
            _netobj = GetComponent<NetworkObject>();
        }

        private void LateUpdate()
        {
            if(_netobj != null && !NetworkManager.Singleton.IsHost)
                return;

            if (!_audioSource.isPlaying)
                Destroy(gameObject);
        }
    }
}
