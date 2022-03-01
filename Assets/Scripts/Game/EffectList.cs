using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;

namespace NoZ.Zisle
{
    // TODO: handle enable / disabling of effects
    // TODO: handle adding / removing of effects

    public class EffectList : NetworkVariableBase
    {
        private static List<EffectStack> _stackPool = new List<EffectStack>();

        public enum ChangeEventType : byte
        {
            Add,
            Remove,
            Clear,
            Enable,
            Disable
        }

        private struct ChangeEvent
        {
            public int Tick;

            public ChangeEventType Type;

            public ulong SourceId;

            public ushort EffectId;

            public ushort ContextIndex;
        }

        private Actor _actor = null;
        private LinkedList<EffectStack> _stacks = new LinkedList<EffectStack>();
        private List<ChangeEvent> _events = new List<ChangeEvent>(64);

        public LinkedList<EffectStack> Stacks => _stacks;

        /// <summary>
        /// Bind the effect list to a specific actor
        /// </summary>
        public EffectList (Actor actor)
        {
            _actor = actor;
        }

        /// <inheritdoc />
        public override void ResetDirty()
        {
            base.ResetDirty();
            _events.Clear();
        }

        /// <inheritdoc />
        public override bool IsDirty()
        {
            // we call the base class to allow the SetDirty() mechanism to work
            return base.IsDirty() || _events.Count > 0;
        }

        public override void WriteField(FastBufferWriter writer)
        {
            //writer.WriteValueSafe((ushort)_contexts.Count);
            //for (int i = 0; i < _contexts.Count; i++)
            //{
            //    var context = _contexts[i];
            //    writer.WriteValueSafe(context.Tick);
            //    writer.WriteValueSafe(context.Effect.NetworkId);
            //}
        }

        public override void ReadField(FastBufferReader reader)
        {
            Clear();

            reader.ReadValueSafe(out ushort count);
            for (int i = 0; i < count; i++)
            {
                reader.ReadValueSafe(out int tick);
                reader.ReadValueSafe(out ushort effectId);

                HandleEvent(new ChangeEvent { EffectId = effectId, Tick = tick, Type = ChangeEventType.Add });
            }
        }

        public override void WriteDelta(FastBufferWriter writer)
        {
            // Write total number of events
            writer.WriteValueSafe((ushort)_events.Count);

            // Write each event
            for (int i = 0; i < _events.Count; i++)
            {
                writer.WriteValueSafe(_events[i].Type);
                writer.WriteValueSafe(_events[i].Tick);
                writer.WriteValueSafe(_events[i].EffectId);

                switch(_events[i].Type)
                {
                    case ChangeEventType.Add:
                        writer.WriteValueSafe(_events[i].SourceId);
                        break;

                    case ChangeEventType.Remove:
                        writer.WriteValueSafe(_events[i].ContextIndex);
                        break;
                }
            }
        }

        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta)
        {
            reader.ReadValueSafe(out ushort eventCount);
            for (var eventIndex = 0; eventIndex < eventCount; eventIndex++)
            {
                reader.ReadValueSafe(out ChangeEventType type);
                reader.ReadValueSafe(out int tick);
                reader.ReadValueSafe(out ushort effectId);

                ulong sourceId = 0;
                ushort contextIndex = 0;
                switch(type)
                {
                    case ChangeEventType.Add:
                        reader.ReadValueSafe(out sourceId);
                        break;

                    case ChangeEventType.Remove:
                        reader.ReadValueSafe(out contextIndex);
                        break;
                }
                
                HandleEvent(new ChangeEvent { 
                    EffectId = effectId, 
                    Tick = tick, 
                    Type = type, 
                    SourceId = sourceId, 
                    ContextIndex = contextIndex 
                });
            }
        }

        public void Add(Actor source, Effect effect)
        {
            if (effect == null || _actor == null)
                return;

            // Add the new effect
            AddEvent(ChangeEventType.Add, effect, sourceId : source.NetworkObjectId);

            // Remove instant effects immediately
            if(effect.Lifetime == EffectLifetime.Instant)
                AddEvent(ChangeEventType.Remove, effect);

            SetDirty(true);
        }

        /// <summary>
        /// Remove all effects that match the given lifetime
        /// </summary>
        public void RemoveEffects(EffectLifetime lifetime)
        {
            var tick = NetworkManager.Singleton.ServerTime.Tick;
            var tickRate = 1.0f / NetworkManager.Singleton.ServerTime.TickRate;

            LinkedListNode<EffectStack> nextStackNode;
            for (var stackNode = _stacks.First; stackNode != null; stackNode = nextStackNode)
            {
                nextStackNode = stackNode.Next;
                var stack = stackNode.Value;

                LinkedListNode<EffectContext> nextContextNode;
                for(var contextNode = stack.Contexts.First; contextNode != null; contextNode = nextContextNode)
                {
                    nextContextNode = contextNode.Next;
                    var context = contextNode.Value;
                    if (lifetime == context.Lifetime && (lifetime != EffectLifetime.Time || (tick - context.Tick) * tickRate >= context.Duration))
                        context.Release(UpdateState);
                }

                if (stack.Contexts.Count == 0)
                    RemoveStack(stack);
            }
        }

        private void AddEvent(ChangeEventType type, Effect effect, ulong sourceId = 0)
        {
            var evt = new ChangeEvent
            {
                Type = type,
                Tick = NetworkManager.Singleton.ServerTime.Tick,
                EffectId = effect.NetworkId,
                SourceId = sourceId
            };
            _events.Add(evt);

            HandleEvent(evt);
        }

        private void HandleEvent(ChangeEvent evt)
        {
            // Convert the sourceId into an Actor
            Actor source = null;
            if (evt.SourceId != 0 && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(evt.SourceId, out var networkObject))
                source = networkObject.GetComponent<Actor>();

            var effect = NetworkScriptableObject<Effect>.Get(evt.EffectId);

            switch (evt.Type)
            {
                case ChangeEventType.Add:
                    {
                        // Create the effect context
                        var context = EffectContext.Get(effect, source, _actor);

                        // Add the effect context to the end of the stack
                        var stack = GetStack(effect);
                        if(null == stack)
                            stack = AddStack(effect);
                        stack.Contexts.AddLast(context.Node);

                        // Handle maximum stacks
                        while(stack.Contexts.Count > effect.MaximumStacks)
                            stack.Contexts.First.Value.Release();

                        for(var componentNode = context.Components.Last; componentNode != null; componentNode = componentNode.Previous)
                        {
                            var component = componentNode.Value;
                            if (component.Tag == null)
                                component.Enabled = true;
                            else
                                UpdateState(component.Tag);
                        }
                        break;
                    }

                case ChangeEventType.Remove:
                    {
                    }
                    break;
            }
        }

        public override void Dispose()
        {
            Clear();
        }

        public void Clear()
        {
            _events.Clear();

            // Free all stacks
            while (_stacks.Count > 0)
                RemoveStack(_stacks.First.Value);
        }

        private void UpdateState (Tag tag)
        {
            if (tag == null)
                return;

            EffectComponentContext componentToEnable = null;

            for(var stackNode = _stacks.Last; stackNode != null; stackNode = stackNode.Previous)
            {
                var stack = stackNode.Value;
                for (var contextNode = stack.Contexts.Last; contextNode != null; contextNode = contextNode.Previous)
                {
                    var context = contextNode.Value;
                    for (var componentNode = context.Components.Last; componentNode != null; componentNode = componentNode.Previous)
                    {
                        var component = componentNode.Value;

                        if (component.Tag == tag)
                        {
                            component.Enabled = false;
                            if (componentToEnable == null)
                                componentToEnable = component;
                        }
                    }
                }
            }

            if (componentToEnable != null)
                componentToEnable.Enabled = true;
        }

        private EffectStack GetStack (Effect effect)
        {
            for (var stackNode = _stacks.First; stackNode != null; stackNode = stackNode.Next)
                if (stackNode.Value.Effect == effect)
                    return stackNode.Value;

            return null;
        }

        private EffectStack AddStack (Effect effect)
        {
            EffectStack stack;
            if (_stackPool.Count > 0)
            {
                stack = _stackPool[_stackPool.Count - 1];
                _stackPool.RemoveAt(_stackPool.Count - 1);
            }
            else
                stack = new EffectStack();

            stack.Effect = effect;

            for(var stackNode = _stacks.First; stackNode != null; stackNode = stackNode.Next)
            {
                if(effect.Priority < stackNode.Value.Effect.Priority)
                {
                    _stacks.AddBefore(stackNode, stack.Node);
                    break;
                }
            }

            if (stack.Node.List == null)
                _stacks.AddLast(stack.Node);

            return stack;
        }

        private void RemoveStack (EffectStack stack)
        {
            if (stack.Node.List != null)
                stack.Node.List.Remove(stack.Node);

            while (stack.Contexts.Count > 0)
                stack.Contexts.First.Value.Release();
        }
    }
}
