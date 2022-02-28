using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public class GlobalCooldownCondition : AbilityCondition
    {
        [SerializeField] private float _duration = 1.5f;

        public override float CheckCondition(Actor source, Ability ability) =>
            (Time.time - source.LastAbilityUsedTime) >= _duration ? 1.0f : 0.0f;
    }
}
