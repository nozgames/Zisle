using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public class CooldownCondition : AbilityCondition
    {
        [SerializeField] private float _duration = 1.0f;

        public override float CheckCondition(Actor source, Ability ability) =>
            (Time.time - source.GetAbilityLastUsedTime(ability)) >= _duration ? 1.0f : 0.0f;
    }
}
