using UnityEngine;

namespace NoZ.Zisle
{
    // TODO: handle actor stopping distances

    /// <summary>
    /// Brain that causes the actor to follow the current path to the home tile.  If the 
    /// Actor is not on a path then path following will resume when a path is encountered again.
    /// </summary>
    [CreateAssetMenu(menuName = "Zisle/Brains/FollowPath")]
    public class FollowPath : Brain
    {
        [Header("Follow Path")]
        [SerializeField] private float _stoppingDistance = 0.2f;

        private class State : IThinkState
        {
            public Game.PathNode NextPathNode;
        }

        public override bool Think (Actor actor, IThinkState thinkState)
        {
            var state = thinkState as State;

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

            return true;
        }

        public override IThinkState AllocThinkState(Actor actor)
        {
            return new State
            {
                NextPathNode = Game.PathNode.Invalid
            };
        }
    }
}
