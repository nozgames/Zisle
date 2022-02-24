using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public class ActorEffectContext
    {
        private static LinkedList<ActorEffectContext> _pool = new LinkedList<ActorEffectContext>();

        public ActorEffect Effect { get; private set; }

        public Actor Target { get; private set; }
        public Actor Source { get; private set; }
        public double StartTime { get; private set; }
        public double Duration { get; private set; }
        public ActorEffectLifetime Lifetime { get; private set; }
        public object UserData { get; set; }

        public LinkedListNode<ActorEffectContext> Node { get; private set; }

        private bool _enabled;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;
                if (_enabled)
                    Effect.Apply(this);
                else
                    Effect.Remove(this);
            }
        }

        public ActorEffectContext()
        {
            Node = new LinkedListNode<ActorEffectContext>(this);
        }


        public static ActorEffectContext Get (ActorEffect effect, Actor source, Actor target)
        {
            ActorEffectContext context;
            if(_pool.Count > 0)
            {
                context = _pool.First.Value;
                _pool.Remove(_pool.First);
            }
            else
            {
                context = new ActorEffectContext();
            }

            context.Effect = effect;
            context.Target = target;
            context.Source = source;
            context.Lifetime = effect.Lifetime;
            context.Duration = effect.Duration;
            context.StartTime = Time.timeAsDouble;
            context.UserData = null;

            return context;
        }

        public void Release ()
        {
            Effect.Release(this);
            Effect = null;
            Target = null;
            Source = null;
            _enabled = false;

            if(Node.List != null)
            {
                Node.List.Remove(Node);
                _pool.AddLast(Node);
            }
        }
    }
}
