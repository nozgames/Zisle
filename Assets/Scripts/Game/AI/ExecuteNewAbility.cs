using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    [CreateAssetMenu(menuName = "Zisle/Lobes/Execute Ability")]
    public class ExecuteNewAbility : Lobe<ExecuteAbility.ThinkState>
    {
        public class ThinkState : IThinkState
        {
            public List<List<Actor>> Targets = null;
            public int BestAbility = -1;

            public void OnAlloc(Actor actor)
            {
                // Create a new target list that has the capacity for each of the actor abilities
                if(Targets == null)
                    Targets = new List<List<Actor>> (actor.Abilities.Length);

                // Allocate a target list for each ability up to the number of abilities this actor has
                for (int i = Targets.Count; i < actor.Abilities.Length; i++)
                    Targets.Add(new List<Actor>());
            }

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
                var score = actor.Abilities[i].CalculateScore(actor, state.Targets[i]);
                if (score > bestScore)
                {
                    bestScore = score;
                    state.BestAbility = i;
                }
                else
                    state.Targets[i].Clear();
            }

            return bestScore;
        }

        public override void Think(Actor actor, IThinkState istate)
        {
            var state = istate as ThinkState;
            if (state.BestAbility < 0)
                return;

            var ability = actor.Abilities[state.BestAbility];
            actor.ExecuteAbility(ability, state.Targets[state.BestAbility]);
            state.Targets[state.BestAbility].Clear();
            state.BestAbility = -1;
        }

        /// <summary>
        /// Overidden to clear out the target cache of the best ability if this lobe was not chosen
        /// </summary>
        public override void DontThink(Actor actor, IThinkState istate)
        {
            base.DontThink(actor, istate);

            // Make sure we dont needlessly hang out to targets for the best ability
            // if this ablity was ultimately not choses.
            var state = istate as ThinkState;
            if(state.BestAbility != -1)
                state.Targets[state.BestAbility].Clear();
        }
    }
}
