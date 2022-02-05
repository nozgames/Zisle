using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Biome")]
    public class Biome : NetworkScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private Material _material = null;
        [SerializeField] private GameObject _bridge = null;

        [Header("Spawning")]
        [SerializeField] private int _minLevel = 2;
        [SerializeField] private int _maxLevel = 100000;
        [SerializeField] private float _weight = 1.0f;

        [Space]
        [SerializeField] private Island[] _islands = null;


        public Material Material => _material;
        public GameObject Bridge => _bridge;

        public int MinLevel => _minLevel;
        public int MaxLevel => _maxLevel;
        public float Weight => _weight;

        public Island[] Islands => _islands;

        public void OnAfterDeserialize()
        {
            foreach (var island in _islands)
                island.Biome = this;
        }

        public void OnBeforeSerialize()
        {
        }
    }
}
