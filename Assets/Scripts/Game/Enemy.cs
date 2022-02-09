using UnityEngine;
using NoZ.Animations;
using Unity.Netcode;
using UnityEngine.AI;
using System.Collections;
using NoZ.Events;

namespace NoZ.Zisle
{
    public class Enemy : Actor
    {
        [SerializeField] private float _updateRate = 1.0f;

        protected override void Awake()
        {
            base.Awake();

            NavAgent.updateRotation = false;
            NavAgent.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost)
            {
                StartCoroutine(UpdateTarget());
            }

            NavAgent.enabled = true;
        }

        public override void UpdateAttributes()
        {
            base.UpdateAttributes();

            NavAgent.speed = GetAttributeValue(ActorAttribute.Speed);
        }


        private IEnumerator UpdateTarget ()
        {
            var wait = new WaitForSeconds(_updateRate);

            while (NetworkManager.Singleton == null || !NavAgent.isOnNavMesh)
                yield return null;

            while(gameObject.activeSelf && !IsDead)
            {
                var player = FindNearestPlayer();
                if (null != player)
                {
                    transform.rotation = Quaternion.LookRotation((player.transform.position.ZeroY() - transform.position.ZeroY()), Vector3.up);

                    var playerDist = (player.transform.position.ZeroY() - transform.position.ZeroY()).sqrMagnitude;
                    if (playerDist >= (NavAgent.stoppingDistance * NavAgent.stoppingDistance))
                    {
                        //                        NavAgent.SetDestination(player.transform.position.ZeroY());
                    }
                    NavAgent.SetDestination(player.transform.position.ZeroY());
                }

                yield return wait;
            }
        }

        private Player FindNearestPlayer ()
        {
            var bestDist = float.MaxValue;
            var bestPlayer = (Player)null;
            foreach(var player in Player.All)
            {
                var dist = (player.transform.position - transform.position).sqrMagnitude;
                if(dist < bestDist)
                {
                    bestDist = dist;
                    bestPlayer = player;
                }
            }

            return bestPlayer;
        }


        public float _runPitchSmooth = 0.2f;
        private float _runPitchSmoothVelocity = 0.05f;

#if false
        private void FixedUpdate()
        {
            // TODO: pitch the character forward when moving


            var speed = NavAgent.desiredVelocity.magnitude; //  (transform.position - _lastPosition).magnitude / Time.fixedDeltaTime;
            _speed = Mathf.SmoothDamp(_speed, speed, ref _runPitchSmoothVelocity, _runPitchSmooth);


            if (_runPitchTransform != null)
            {
                var normalizedSpeed = Mathf.Clamp01(_speed / GetAttributeValue(ActorAttribute.Speed));
                _runPitchTransform.localRotation = Quaternion.Euler(_runPitch * normalizedSpeed, 0, 0);
            }

            _lastPosition = transform.position;
        }
#endif
    }
}
