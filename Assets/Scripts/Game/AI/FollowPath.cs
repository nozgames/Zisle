using UnityEngine;

namespace NoZ.Zisle
{
    // TODO: handle actor stopping distances

    /// <summary>
    /// Brain that causes the actor to follow the current path to the home tile.  If the 
    /// Actor is not on a path then path following will resume when a path is encountered again.
    /// </summary>
    [CreateAssetMenu(menuName = "Zisle/Lobes/Follow Path")]
    public class FollowPath : Lobe<FollowPath.ThinkState>
    {
        [Header("Follow Path")]
        [SerializeField] private float _stoppingDistance = 0.2f;

        public class ThinkState : IThinkState
        {
            public Game.PathNode NextPathNode;

            public void OnAlloc(Actor actor) => NextPathNode = Game.PathNode.Invalid;
            public void OnRelease() { }
        }

        public override float CalculateScore(Actor actor, IThinkState state) =>
            actor.State == ActorState.Active ? 1.0f : 0.0f;

        public override void Think (Actor actor, IThinkState istate)
        {
            var state = istate as ThinkState;
            var pathNode = Game.Instance.WorldToPathNode(actor.transform.position);
            var withinRange = actor.NavAgent.remainingDistance <= _stoppingDistance;

            // New path
            if (pathNode.IsPath && (!state.NextPathNode.IsPath || (state.NextPathNode.IsPath && withinRange && pathNode.To != state.NextPathNode.To)))
            {
                state.NextPathNode = pathNode;
                actor.SetDestination(TileGrid.CellToWorld(pathNode.To), stoppingDistance: _stoppingDistance);
            }
            // Continue path
            else if (state.NextPathNode.IsPath && !withinRange)
            {
                actor.SetDestination(TileGrid.CellToWorld(state.NextPathNode.To), stoppingDistance: _stoppingDistance);
            }
            // Move towards base in hopes that we will find a path
            else
            {
                state.NextPathNode = Game.PathNode.Invalid;
                actor.SetDestination(Vector3.zero, stoppingDistance: _stoppingDistance);
            }

            // Look at the path
            actor.LookAt(actor.NavAgent.destination);            
        }
    }
}
