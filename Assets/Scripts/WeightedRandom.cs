using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public static class WeightedRandom
    {
        private static List<float> _weights = new List<float>(1024);

        /// <summary>
        /// Pick a random index in a weighted array
        /// </summary>
        public static int RandomWeightedIndex<T>(IEnumerable<T> items, int start, int count, System.Func<T, float> getWeight)
        {
            var totalWeight = 0.0f;
            _weights.Clear();
            foreach (var item in items)
            {
                if (start > 0)
                {
                    start--;
                    continue;
                }

                var weight = Mathf.Max(getWeight(item), 0.0f);
                _weights.Add(weight);
                totalWeight += weight;

                if (--count == 0)
                    break;
            }

            if (_weights.Count == 0)
                return 0;

            // Chooose a number between 0 and totalWeight and find the item that falls in that range
            var random = Random.Range(0.0f, totalWeight);
            var choice = 0;
            for (; choice < _weights.Count - 1 && random > _weights[choice] - float.Epsilon; choice++)
                random -= _weights[choice];

            return choice;
        }
    }
}
