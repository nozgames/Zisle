using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public class GameOptions
    {
        public int MaxIslands = 64;
        public bool SpawnEnemies = true;

        public int StartingPaths = 1;

        [Header("Path Weights")]
        public float PathWeight0 = 0.1f;
        public float PathWeight1 = 1.0f;
        public float PathWeight2 = 1.0f;
        public float PathWeight3 = 0.25f;

        public IEnumerable<float> GetForkWeights()
        {
            yield return PathWeight0;
            yield return PathWeight1;
            yield return PathWeight2;
            yield return PathWeight3;
        }
    }
}
