namespace NoZ.Zisle
{
    public enum TargetType
    {
        /// <summary>
        /// The effect is applied to the source 
        /// </summary>
        Self,

        /// <summary>
        /// The effect is applied to a custom set of targets defined by a target finder
        /// </summary>
        Custom,

        /// <summary>
        /// The effect is applied to all targets of the parent.  In the case of a group effect it is 
        /// the targets the group effect is being applied to and in the case of an Ability event it is 
        /// the targets of the ability event.
        /// </summary>
        Inherit
    }
}
