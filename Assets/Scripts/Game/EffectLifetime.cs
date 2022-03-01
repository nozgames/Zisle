namespace NoZ.Zisle
{
    public enum EffectLifetime
    {
        /// <summary>
        /// Specific amount of time
        /// </summary>
        Time,

        /// <summary>
        /// Effect will add and apply and immediately remove
        /// </summary>
        Instant,

        /// <summary>
        /// Duration of an ability
        /// </summary>
        Ability,

        /// <summary>
        /// Leave the effect on until another ability is used
        /// </summary>
        NextAbility,

        /// <summary>
        /// Never remove the effect
        /// </summary>
        Infinite
    }
}
