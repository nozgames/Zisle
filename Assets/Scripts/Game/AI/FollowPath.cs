using UnityEngine;

namespace NoZ.Zisle
{
    // TODO: handle actor stopping distances

    /// <summary>
    /// Brain that causes the actor to follow the current path to the home tile.  If the 
    /// Actor is not on a path then path following will resume when a path is encountered again.
    /// </summary>
    [CreateAssetMenu(menuName = "Zisle/Lobes/FollowPath")]
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

        public override float CalculateScore(Actor actor, IThinkState state) => 1.0f;

        public override void Think (Actor actor, IThinkState istate)
        {
            var state = istate as ThinkState;

            // Current path node
            var pathNode = Game.Instance.WorldToPathNode(actor.transform.position);

            // Were we already following the path?            
            if(state.NextPathNode.IsPath)
            {
                // New destination?
                if(pathNode.IsPath && pathNode.To != state.NextPathNode.To && actor.NavAgent.remainingDistance <= _stoppingDistance)
                {
                    state.NextPathNode = pathNode;
                    actor.SetDestination(TileGrid.CellToWorld(pathNode.To), stoppingDistance:_stoppingDistance);
                }
                // If we are not on the path then head back towards the last path we were heading towards
                else
                {
                    actor.SetDestination(TileGrid.CellToWorld(state.NextPathNode.To), stoppingDistance: _stoppingDistance);
                }
            } 
            // If we have a path now then head towards it
            else if (pathNode.IsPath)
            {
                state.NextPathNode = pathNode;
                actor.SetDestination(TileGrid.CellToWorld(pathNode.To), stoppingDistance:_stoppingDistance);
            }
            // If not on the path then head towards the home tile instead in hopes we find a path
            else
            {
                // TODO: too hard coded
                state.NextPathNode = Game.PathNode.Invalid;
                actor.SetDestination(Vector3.zero, stoppingDistance: _stoppingDistance);
            }

            // Look at the path
            actor.LookAt(actor.NavAgent.destination);            
        }
    }
}
