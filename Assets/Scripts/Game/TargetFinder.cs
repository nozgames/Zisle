using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public abstract class TargetFinder : ScriptableObject
    {
        private class SelfTargetFinder : TargetFinder
        {
            protected override void AddTargets(Actor source) => Add(source);
        }

        private class EmptyTargetFinder : TargetFinder
        {
            protected override void AddTargets(Actor source) { }
        }

        private static SelfTargetFinder _selfTarget;
        private static EmptyTargetFinder _emptyTarget;

        private Actor _source = null;
        private int _frame = 0;
        private List<Actor> _targets = new List<Actor>();

        /// <summary>
        /// List of cached targets
        /// </summary>
        public List<Actor> Targets => _targets;

        /// <summary>
        /// Number of cached targets
        /// </summary>
        public int Count => _targets.Count;

        /// <summary>
        /// True if the TargetFinder has any cached targets
        /// </summary>
        public bool HasTargets => _targets.Count > 0;

        /// <summary>
        /// Clear all cached targets for this target finder
        /// </summary>
        public void Clear ()
        {
            _frame = 0;
            _source = null;
            _targets.Clear();
        }

        private static TargetFinder GetSelfTargetFinder(Actor source)
        {
            if (_selfTarget == null)
                _selfTarget = CreateInstance<SelfTargetFinder>();

            _selfTarget.Clear();
            _selfTarget.FindTargets(source);

            return _selfTarget;
        }

        private static TargetFinder GetEmptyTargetFinder()
        {
            if(_emptyTarget == null)
                _emptyTarget = CreateInstance<EmptyTargetFinder>();

            return _emptyTarget;
        }

        /// <summary>
        /// Find all targets for the given actor source
        /// </summary>
        /// <param name="source"></param>
        public int FindTargets (Actor source)
        {
            // Already cached the targets?
            if (source == _source && _frame == Time.frameCount)
                return _targets.Count;

            Clear();
            AddTargets(source);

            return _targets.Count;
        }

        /// <summary>
        /// Add a target to the cached targets list
        /// </summary>
        protected void Add(Actor target)
        {
            _targets.Add(target);
        }

        /// <summary>
        /// Implement to add targets for the given source
        /// </summary>
        protected abstract void AddTargets (Actor source);

        /// <summary>
        /// Find targets for the given source handling self targetting and inheritance
        /// </summary>
        public static TargetFinder FindTargets(Actor source, TargetType type, TargetFinder custom, TargetFinder inherit=null)
        {
            switch (type)
            {
                case TargetType.Custom:
                    if (custom != null)
                    {
                        custom.FindTargets(source);
                        return custom;
                    }

                    if (_emptyTarget != null)
                        _emptyTarget = new EmptyTargetFinder();
                    return _emptyTarget;

                default:
                case TargetType.Inherit:
                    return inherit != null ? inherit : GetEmptyTargetFinder();

                case TargetType.Self:
                    return GetSelfTargetFinder(source);
            }
        }
    }
}
