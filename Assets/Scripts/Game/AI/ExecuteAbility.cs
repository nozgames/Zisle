using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Lobes/Execute Ability")]
    public class ExecuteAbility : Lobe<ExecuteAbility.ThinkState>
    {
        public class ThinkState : IThinkState
        {
            public int BestAbility = -1;

            public void OnAlloc(Actor actor) { }
            public void OnRelease() { }
        }

        public override float CalculateScore(Actor actor, IThinkState istate)
        {
            if (actor.IsBusy)
                return 0.0f;

            var state = (istate as ThinkState);
            var bestScore = float.MinValue;
            for (int i = 0; i < actor.Abilities.Length; i++)
            {
                if (actor.Abilities[i] == null)
                    continue;

                var score = actor.Abilities[i].CalculateScore(actor);
                if (score <= bestScore)
                    continue;

                bestScore = score;
                state.BestAbility = i;
            }

            return bestScore;
        }

        public override void Think(Actor actor, IThinkState istate)
        {
            var state = istate as ThinkState;
            if (state.BestAbility < 0)
                return;

            var ability = actor.Abilities[state.BestAbility];
            actor.ExecuteAbility(ability);
            state.BestAbility = -1;
        }
    }
}
