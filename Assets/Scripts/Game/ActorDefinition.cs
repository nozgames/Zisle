using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Actor Definition")]
    public class ActorDefinition : NetworkScriptableObject
    {
        [Header("General")]
        [SerializeField] private ActorType _type = ActorType.Player;
        [SerializeField] private string _displayName = null;
        [SerializeField] private GameObject _prefab = null;
        [SerializeField] private GameObject _preview = null;

        [Header("Attributes")]
        [SerializeField] private float _baseHealthMax = 100.0f;
        [SerializeField] private float _baseHealthRegen = 5.0f;
        [SerializeField] private float _baseSpeed = 10.0f;
        [SerializeField] private float _baseAttack = 60.0f;
        [SerializeField] private float _baseAttackSpeed = 1.0f;
        [SerializeField] private float _baseDefense = 50.0f;
        [SerializeField] private float _baseHarvest = 1.0f;
        [SerializeField] private float _baseBuild = 1.0f;

        [Space]
        [SerializeField] private ActorAbility[] _abilities = null;

        /// <summary>
        /// Get the available abilities for this actor
        /// </summary>
        public ActorAbility[] Abilities => _abilities;

        /// <summary>
        /// Get the prefab used to spawn the player
        /// </summary>
        public GameObject Prefab => _prefab;

        /// <summary>
        /// Prefab to use for preview
        /// </summary>
        public GameObject Preview => _preview;

        /// <summary>
        /// Name to use in UI
        /// </summary>
        public string DisplayName => _displayName ?? name;

        /// <summary>
        /// Get the actor type
        /// </summary>
        public ActorType ActorType => _type;

        /// <summary>
        /// Return the base attribute for the given attribute
        /// </summary>
        public float GetBaseAttribute(ActorAttribute attribute) => attribute switch
        {
            ActorAttribute.HealthMax => _baseHealthMax,
            ActorAttribute.HealthRegen => _baseHealthRegen,
            ActorAttribute.Speed => _baseSpeed,
            ActorAttribute.Attack => _baseAttack,
            ActorAttribute.AttackSpeed => _baseAttackSpeed,
            ActorAttribute.Defense => _baseDefense,
            ActorAttribute.Harvest => _baseHarvest,
            ActorAttribute.Build => _baseBuild,
            _ => throw new System.NotImplementedException()
        };


        public override void RegisterNetworkId()
        {
            base.RegisterNetworkId();

            foreach (var ability in _abilities)
                ability.RegisterNetworkId();
        }
    }
}