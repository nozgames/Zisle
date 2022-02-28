using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public class ActorStateCondition : AbilityCondition
    {
        [SerializeField] private ActorState _state = ActorState.Active;

        public override float CheckCondition(Actor source, Ability ability) =>
            source.State == _state ? 1.0f : 0.0f;
    }
}
