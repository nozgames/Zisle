using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace NoZ.Zisle
{
    /// <summary>
    /// Represents an effect that has been applied to an actor
    /// </summary>
    public class EffectContext
    {        
        /// <summary>
        /// Pool of effect contexts
        /// </summary>
        private static LinkedList<EffectContext> _pool = new LinkedList<EffectContext> ();
        private static uint _nextId = 1;

        private LinkedList<EffectComponentContext> _components = new LinkedList<EffectComponentContext>();

        public Actor Target { get; private set; }
        public Actor Source { get; private set; }
        public int Tick { get; private set; }
        public double Duration { get; set; }
        public EffectLifetime Lifetime { get; set; }

        public uint Id { get; private set; }

        /// <summary>
        /// Effect the context represents
        /// </summary>
        public Effect Effect { get; private set; }

        public LinkedList<EffectComponentContext> Components => _components;

        public LinkedListNode<EffectContext> Node { get; private set; }

        public int ComponentCount => _components.Count;

        public EffectContext()
        {
            Node = new LinkedListNode<EffectContext>(this);
        }

        /// <summary>
        /// Get a new Effect Context from the pool
        /// </summary>
        public static EffectContext Get (Effect effect, Actor source, Actor target, uint contextId = 0)
        {
            // Get a context from the pool or allocate a new one
            EffectContext context;
            if (_pool.Count > 0)
            {
                context = _pool.First.Value;
                _pool.Remove(context.Node);
            }
            else
            {
                context = new EffectContext();
            }

            context.Effect = effect;
            context.Target = target;
            context.Source = source;
            context.Lifetime = effect.Lifetime;
            context.Duration = effect.Duration;
            context.Id = contextId == 0 ? _nextId++ : contextId;
            context.Tick = NetworkManager.Singleton.ServerTime.Tick;

            // Create a component context for each component in the effect
            foreach (var component in effect.Components)
                context._components.AddLast(EffectComponentContext.Get(context, component).Node);

            return context;
        }

        public void Release (Action<Tag> onRelease=null)
        {
            // Release all components
            for (int i = _components.Count - 1; i >= 0; i--)
            {
                var tag = _components.First.Value.Tag;
                _components.First.Value.Release();
                if (tag != null)
                    onRelease?.Invoke(tag);
            }

            if (_components.Count != 0)
                Debug.Log("hmm");

            Effect = null;
            Target = null;
            Source = null;
            Tick = 0;

            // Remove the effect context from the list it is currently included in
            if (Node.List != null)
                Node.List.Remove(Node);

            // Add the effect context to the pool to reuse it
            _pool.AddLast(Node);
        }
    }
}
