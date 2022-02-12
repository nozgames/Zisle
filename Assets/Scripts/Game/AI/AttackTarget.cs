using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Brains/Attack Target")]
    public class AttackTarget : Brain
    {
        [Header("Attack Target")]
        [SerializeField] private float _aggroRange = 2.0f;

        public override bool Think(Actor actor, IThinkState state)
        {
            var player = FindNearestPlayer(actor);
            if(player != null)
            {
                actor.SetDestination(player.transform.position);
                actor.LookAt(player);
                return false;
            }

            return true;
        }

        private Player FindNearestPlayer(Actor actor)
        {
            var bestDist = float.MaxValue;
            var bestPlayer = (Player)null;
            foreach (var player in Player.All)
            {
                var dist = (player.transform.position - actor.transform.position).sqrMagnitude;
                if (dist < bestDist && dist <= _aggroRange)
                {
                    bestDist = dist;
                    bestPlayer = player;
                }
            }

            return bestPlayer;
        }
    }
}
