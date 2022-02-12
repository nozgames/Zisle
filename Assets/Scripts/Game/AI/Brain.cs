using UnityEngine;

namespace NoZ.Zisle
{
    public interface IThinkState { }

    /// <summary>
    /// Defines an abstract Brain to control an actor
    /// </summary>
    public abstract class Brain : ScriptableObject
    {
        /// <summary>
        /// Think for a single frame.  Return true to continue thinking with other brains or false to stop now.
        /// </summary>
        /// <returns></returns>
        public abstract bool Think(Actor actor, IThinkState state);

        /// <summary>
        /// Create an optional think state to be passed to think
        /// </summary>
        public virtual IThinkState AllocThinkState(Actor actor) => null;

        /// <summary>
        /// Optionally release a think state that was previously created from this brain
        /// </summary>
        public virtual void ReleaseThinkState(IThinkState state) { }
    }
}
