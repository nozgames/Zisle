using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// WARNING: DO NOT REORDER THESE!!!
    /// 
    /// List of actor types.  
    /// 
    /// Note that there must be matching entries in the LayerMask as well
    /// </summary>
    public enum ActorType
    {
        /// <summary>
        /// Player
        /// </summary>
        Player,

        /// <summary>
        /// Player's base and ultimate goal of enemy attacks
        /// </summary>
        Base,

        /// <summary>
        /// Building built by players
        /// </summary>
        Building,

        /// <summary>
        /// Enemy that tries to attack player and the base
        /// </summary>
        Enemy,

        /// <summary>
        /// Harvestable actor such as a tree
        /// </summary>
        Harvestable
    }

    [System.Flags]
    public enum ActorTypeMask
    {
        None = 0,
        Player = 1 << ActorType.Player,
        Base = 1 << ActorType.Base,
        Building = 1 << ActorType.Building,
        Enemy = 1 << ActorType.Enemy,
        Harvestable = 1 << ActorType.Harvestable
    }

    public static class ActorTypeMaskExtensions
    {
        private static readonly int _shift = LayerMask.NameToLayer("Player");

        public static LayerMask ToLayerMask(this ActorTypeMask mask) => (LayerMask)((int)mask << _shift);

        public static ActorTypeMask ToMask(this ActorType type) => (ActorTypeMask)(1 << (int)type);

        public static LayerMask ToLayerMask(this ActorType type) => ToLayerMask(ToMask(type));

        public static int ToLayer(this ActorType type) => _shift + (int)type;
    }
}
