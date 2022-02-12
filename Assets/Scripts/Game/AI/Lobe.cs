using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public interface IThinkState
    {
        void OnAlloc(Actor actor);

        void OnRelease();
    }

    /// <summary>
    /// Defines an abstract Brain Lobe to control an actor
    /// </summary>
    public abstract class Lobe : ScriptableObject
    {
        [Tooltip("Base score for the lobe")]
        [SerializeField] private float _baseScore = 1.0f;

        /// <summary>
        /// Base score of the lobe, multiplied against the calculated score
        /// </summary>
        public float BaseScore => _baseScore;

        /// <summary>
        /// Calculate a score for this lobe, if the lobe is later chosen any state that was 
        /// calculated during this call can be reused
        /// </summary>
        public abstract float CalculateScore(Actor actor, IThinkState state);

        /// <summary>
        /// Think for a single frame.
        /// </summary>
        public abstract void Think(Actor actor, IThinkState state);

        /// <summary>
        /// Create an optional think state to be passed to think
        /// </summary>
        public virtual IThinkState AllocThinkState(Actor actor) => null;

        /// <summary>
        /// Optionally release a think state that was previously created from this brain
        /// </summary>
        public virtual void ReleaseThinkState(IThinkState state) { }
    }

    /// <summary>
    /// Defines an abstract Lobe that uses a pooled Think State 
    /// </summary>
    public abstract class Lobe<TThinkState> : Lobe where TThinkState : class, IThinkState, new()
    {
        private List<TThinkState> _pool = new List<TThinkState>();

        /// <summary>
        /// Create an optional think state to be passed to think
        /// </summary>
        public override IThinkState AllocThinkState(Actor actor)
        {
            TThinkState state = default;

            if (_pool.Count > 0)
            {
                state = _pool[_pool.Count - 1];
                _pool.RemoveAt(_pool.Count - 1);
            }
            else
                state = new TThinkState();

            state.OnAlloc(actor);

            return state;
        }

        /// <summary>
        /// Release a think state that was created with this brain
        /// </summary>
        public override void ReleaseThinkState(IThinkState state)
        {
            _pool.Add(state as TThinkState);
            state.OnRelease();
        }
    }
}
