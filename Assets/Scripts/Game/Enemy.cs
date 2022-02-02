using UnityEngine;
using NoZ.Animations;
using Unity.Netcode;

namespace NoZ.Zisle
{
    public class Enemy : NetworkBehaviour
    {
        [Header("Animation")]
        [SerializeField] private BlendedAnimationController _animator = null;
        [SerializeField] private AnimationShader _idleAnimation = null;



        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            _animator.Play(_idleAnimation);
        }
    }
}
