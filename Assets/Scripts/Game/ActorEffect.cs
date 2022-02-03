using UnityEngine;

namespace NoZ.Zisle
{
    public enum ActorEffectTarget
    {
        Self,
        Target
    }

    public abstract class ActorEffect : NetworkScriptableObject
    {
        public class Context
        {
            public ActorEffect Effect;
            public Actor Target;
            public Actor Source;
            public double Time;
            public object UserData;
        }

        public abstract void Apply(Context context);
        public abstract void Remove(Context context);

        [Tooltip("Target of the effect when applied")]
        [SerializeField] private ActorEffectTarget _target = ActorEffectTarget.Target;

        public ActorEffectTarget Target => _target;

        public virtual void ExecuteOnClent(Actor source, Actor target) => throw new System.NotImplementedException();

        // TODO: visuals, names, etc?
    }
}
