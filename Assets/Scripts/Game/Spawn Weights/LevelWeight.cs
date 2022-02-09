using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Returns a weight based on the current game level
    /// </summary>
    [CreateAssetMenu(menuName = "Zisle/Spawn Weights/Level")]
    public class LevelWeight : SpawnWeight
    {
        [SerializeField] AnimationCurve _curve = null;

        public override float GetWeight(ActorDefinition actorDefinition) =>
            _curve.Evaluate((float)Game.Instance.Level);
    }
}
