using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Biome")]
    public class Biome : NetworkScriptableObject<Biome>, ISerializationCallbackReceiver
    {
        [SerializeField] private Material _material = null;
        [SerializeField] private Bridge _bridge = null;

        [Header("Spawning")]
        [SerializeField] private int _minLevel = 2;
        [SerializeField] private int _maxLevel = 100000;
        [SerializeField] private float _weight = 1.0f;

        [Space]
        [SerializeField] private IslandMesh[] _islands = null;


        public Material Material => _material;
        public Bridge Bridge => _bridge;

        public int MinLevel => _minLevel;
        public int MaxLevel => _maxLevel;
        public float Weight => _weight;

        public IslandMesh[] Islands => _islands;

        public void OnAfterDeserialize()
        {
            foreach (var island in _islands)
                island.Biome = this;
        }

        public void OnBeforeSerialize()
        {
        }

        /// <summary>
        /// Return the index of the island within the biome or -1 if it would not be found
        /// </summary>
        public int IndexOf (IslandMesh island)
        {
            for (int i = 0; i < _islands.Length; i++)
                if (_islands[i] == island)
                    return i;

            return -1;
        }
    }
}
