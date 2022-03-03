using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Lobes/Follow Target")]
    public class FollowTarget : Lobe
    {
        [Header("Follow Target")]
        [SerializeField] private TargetFinder _targetFinder = null;

        public override float CalculateScore(Actor actor, IThinkState istate)
        {
            if (actor.State != ActorState.Active)
                return 0.0f;

            if (0 == _targetFinder.FindTargets(actor))
                return 0.0f;

            return 1.0f;
        }

        public override void Think(Actor actor, IThinkState state)
        {
            if (_targetFinder.Targets.Count == 0)
                return;

            actor.SetDestination(new Destination(_targetFinder.Targets[0]));
        }
    }
}
