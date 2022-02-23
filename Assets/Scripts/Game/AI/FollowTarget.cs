using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Lobes/Follow Target")]
    public class FollowTarget : Lobe<FollowTarget.ThinkState>
    {
        public class ThinkState : IThinkState
        {
            public Actor Follow;

            public void OnAlloc(Actor actor) { }
            public void OnRelease() { Follow = null; }
        }

        [Header("Follow Target")]
        [SerializeField] private ActorTypeMask _followType = ActorTypeMask.Player;
        [SerializeField] private float _followRange = 2.0f;
        [SerializeField] private float _stoppingDistance = 0.7f;

        private static List<Actor> _actors = new List<Actor>();

        public override float CalculateScore(Actor actor, IThinkState istate)
        {
            if (actor.State != ActorState.Active)
                return 0.0f;

            var follow = Game.Instance.FindClosestActor(actor.transform.position, _followRange, _followType);
            if (null == follow)
                return 0.0f;

            (istate as ThinkState).Follow = follow;

            // TODO: increase score the closer they are?
            return 1.0f;
        }

        public override void Think(Actor actor, IThinkState state)
        {
            var thinkState = state as ThinkState;

            actor.SetDestination(thinkState.Follow.transform.position, stoppingDistance: _stoppingDistance);
            actor.LookAt(thinkState.Follow);
        }
    }
}
