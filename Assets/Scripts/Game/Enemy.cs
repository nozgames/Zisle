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

        private Game.PathNode _lastPath;

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

                _lastPath = new Game.PathNode { IsPath = true, To = new Vector2Int(-1000,-1000)};
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
#if false
                var player = FindNearestPlayer();
                if (null != player)
                {
                    

                    var playerDist = (player.transform.position.ZeroY() - transform.position.ZeroY()).sqrMagnitude;
                    if (playerDist >= (NavAgent.stoppingDistance * NavAgent.stoppingDistance))
                    {
                        //                        NavAgent.SetDestination(player.transform.position.ZeroY());
                    }

                    // Follow the path!
                    var currentPath = Game.Instance.WorldToPathNode(transform.position);
                    if(currentPath.IsPath && (_lastPath.To != currentPath.To || !_lastPath.IsPath))
                    {
                        NavAgent.SetDestination(TileGrid.CellToWorld(currentPath.To).ZeroY()); // player.transform.position.ZeroY());
                        NavAgent.stoppingDistance = 0.001f;
                        _lastPath = currentPath;
                    }
                    else if (_lastPath.IsPath && !currentPath.IsPath)
                    {
                        _lastPath = new Game.PathNode();
                        NavAgent.stoppingDistance = 0.9f;
                        NavAgent.SetDestination(Vector3.zero);
                    }

                    // TODO: smooth this out a bit
                    transform.rotation = Quaternion.LookRotation((NavAgent.destination.ZeroY() - transform.position.ZeroY()), Vector3.up);
                }
#endif

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
