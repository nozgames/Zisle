using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public abstract class ActorAbilityCondition : ScriptableObject
    {
        public abstract float CheckCondition(Actor source, ActorAbility ability);

        public abstract float CheckCondition(Actor source, ActorAbility ability, List<Actor> targets);
    }
}
