using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Lobes/Find Target")]
    public class FindTarget : Lobe
    {
        [SerializeField] private TargetFinder _targetFinder = null;

        public override float CalculateScore(Actor source, IThinkState state)
        {
            if (_targetFinder.FindTargets(source) == 0)
                return 0.0f;

            if (_targetFinder.Targets[0] == source.Target)
                return 0.0f;

            return 1.0f;
        }

        public override void Think(Actor source, IThinkState state)
        {
            if (_targetFinder.Count == 0)
                return;

            source.SetDestination(new Destination(_targetFinder.Targets[0]));
        }
    }
}
