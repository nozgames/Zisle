using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public abstract class TargetFinder : ScriptableObject
    {
        public abstract void FindTargets(Actor source, List<Actor> targets);
    }
}
