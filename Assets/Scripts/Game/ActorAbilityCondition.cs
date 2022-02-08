using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public abstract class ActorAbilityCondition : ScriptableObject
    {
        public abstract bool CheckCondition(Actor source, ActorAbility ability);

        public abstract bool CheckCondition(Actor source, ActorAbility ability, List<Actor> targets);
    }
}
