using UnityEngine;

namespace NoZ.Zisle
{
    public class ButtonPressCondition : AbilityCondition
    {
        [SerializeField] private PlayerButton _button = PlayerButton.Primary;

        public override float CheckCondition(Actor source, Ability ability) =>
            (source is Player player && player.WasButtonPressed(_button)) ? 1.0f : 0.0f;
    }
}
