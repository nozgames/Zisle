using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Conditions/Button Press")]
    public class ButtonPressCondition : ActorAbilityCondition
    {
        [SerializeField] private PlayerButton _button = PlayerButton.Action;

        public override float CheckCondition(Actor source, ActorAbility ability)
        {
            if (source is Player player)
                if (player.WasButtonPressed(_button))
                    return 1.0f;

            return 0.0f;
        }

        public override float CheckCondition(Actor source, ActorAbility ability, List<Actor> targets) => 1.0f;
    }
}
