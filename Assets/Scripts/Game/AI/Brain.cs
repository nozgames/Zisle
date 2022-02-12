using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Defines an abstract Brain to control an actor
    /// </summary>
    [CreateAssetMenu(menuName = "Zisle/Brain")]
    public class Brain : ScriptableObject
    {
        private class ThinkState : IThinkState
        {
            public IThinkState[] LobeThinkStates;

            public void OnAlloc(Actor actor) { }
            public void OnRelease() { }
        }

        [SerializeField] private Lobe[] _lobes = null;
        
        private List<ThinkState> _thinkStatePool = new List<ThinkState>();

        /// <summary>
        /// Allocate a pooled think state for this brain
        /// </summary>
        public IThinkState AllocThinkState (Actor actor)
        {
            ThinkState state;
            if(_thinkStatePool.Count > 0)
            {
                state = _thinkStatePool[_thinkStatePool.Count - 1];
                _thinkStatePool.RemoveAt(_thinkStatePool.Count - 1);
            }
            else
                state = new ThinkState { LobeThinkStates = new IThinkState[_lobes.Length] };

            for (int i = 0; i < state.LobeThinkStates.Length; i++)
                state.LobeThinkStates[i] = _lobes[i].AllocThinkState(actor);

            return state;
        }

        /// <summary>
        /// Release a think state back to the pool 
        /// </summary>
        public void ReleaseThinkState (IThinkState state)
        {
            // Release all of the lobe think states
            var thinkState = state as ThinkState;
            for (int i = 0; i < thinkState.LobeThinkStates.Length; i++)
            {
                _lobes[i].ReleaseThinkState(thinkState.LobeThinkStates[i]);
                thinkState.LobeThinkStates[i] = null;
            }

            state.OnRelease();
            _thinkStatePool.Add(thinkState);
        }

        /// <summary>
        /// Think for a single frame.
        /// </summary>
        public void Think(Actor actor, IThinkState state)
        {
            // TODO: for each lobe calc score, find best score

            var bestScore = float.MinValue;
            var bestLobe = -1;
            var thinkState = state as ThinkState;

            for(int i=0; i<_lobes.Length; i++)
            {
                var lobe = _lobes[i];
                var score = lobe.BaseScore * lobe.CalculateScore(actor, thinkState.LobeThinkStates[i]);
                if (score < float.Epsilon)
                    continue;

                if(score > bestScore)
                {
                    bestScore = score;
                    bestLobe = i;
                }
            }

            if (bestLobe != -1)
                _lobes[bestLobe].Think(actor, thinkState.LobeThinkStates[bestLobe]);
        }
    }
}
