using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Lobes/Attack Target")]
    public class AttackTarget : Lobe<AttackTarget.ThinkState>
    {
        public class ThinkState : IThinkState
        {
            public Actor Follow;

            public void OnAlloc(Actor actor) { }
            public void OnRelease() { Follow = null; }
        }

        [Header("Attack Target")]
        [SerializeField] private float _aggroRange = 2.0f;

        public override float CalculateScore(Actor actor, IThinkState state)
        {
            var player = FindNearestPlayer(actor);
            if (null == player)
                return 0.0f;

            (state as ThinkState).Follow = player;

            // TODO: increase score the closer they are?
            return 1.0f;
        }

        public override void Think(Actor actor, IThinkState state)
        {
            var thinkState = state as ThinkState;

            actor.SetDestination(thinkState.Follow.transform.position);
            actor.LookAt(thinkState.Follow);
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
