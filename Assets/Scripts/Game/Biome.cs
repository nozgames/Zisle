using System.Linq;
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

        [Space]
        [SerializeField] private ActorDefinition[] _actors = null;

        private ActorDefinition[] _enemies = null;
        private ActorDefinition[] _harvestables = null;

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

            if(_actors == null)
                _actors = new ActorDefinition[0];
        }

        public void OnEnable()
        {
            foreach (var actor in _actors)
                Debug.Log(actor.ActorType);

            _enemies = _actors.Where(a => a != null && a.ActorType == ActorType.Enemy).ToArray();
            _harvestables = _actors.Where(a => a != null && a.ActorType == ActorType.Harvestable).ToArray();

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
        
        /// <summary>
        /// Choose a weighted random enemy to spawn from the list of available enemies.  
        /// </summary>
        public ActorDefinition ChooseRandomEnemy ()
        {
            if (_enemies.Length == 0)
                return null;

            return _enemies[WeightedRandom.RandomWeightedIndex(_enemies, 0, _enemies.Length, GetActorWeight)];
        }

        /// <summary>
        /// Choose a weighted random harvestable to spawn from the list of available harvestables.  
        /// </summary>
        public ActorDefinition ChooseRandomHarvestable()
        {
            if (_harvestables.Length == 0)
                return null;

            return _harvestables[WeightedRandom.RandomWeightedIndex(_harvestables, 0, _harvestables.Length, GetActorWeight)];
        }

        private float GetActorWeight(ActorDefinition def) => def.GetSpawnWeight();
    }
}
