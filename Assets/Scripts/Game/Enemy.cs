using UnityEngine;
using NoZ.Animations;
using Unity.Netcode;
using UnityEngine.AI;
using System.Collections;

namespace NoZ.Zisle
{
    public class Enemy : Actor
    {
        private NavMeshAgent _agent;
        [SerializeField] private float _updateRate = 1.0f;

        protected override void Awake()
        {
            base.Awake();

            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false;
            _agent.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost)
            {
                StartCoroutine(UpdateTarget());
                _agent.enabled = true;
            }
        }

        public override void UpdateAttributes()
        {
            base.UpdateAttributes();

            _agent.speed = GetAttributeValue(ActorAttribute.Speed);
        }

        public override void Die()
        {
            _agent.enabled = false;            

            base.Die();
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
                    if (playerDist >= (_agent.stoppingDistance * _agent.stoppingDistance))
                    {
//                        _agent.SetDestination(player.transform.position.ZeroY());
                    }
                    _agent.SetDestination(player.transform.position.ZeroY());

#if false
                    if ((transform.position - player.transform.position).magnitude < _attackRange)
                    {
                        if(_agent.hasPath)
                            _agent.SetDestination(transform.position);
                        State = ActorState.Idle;
                    }
                    else if ((transform.position - player.transform.position).magnitude > _attackRange + float.Epsilon)
                    {
                        _agent.SetDestination(player.transform.position);
                        State = ActorState.Run;
                    }
#endif
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


            var speed = _agent.desiredVelocity.magnitude; //  (transform.position - _lastPosition).magnitude / Time.fixedDeltaTime;
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
