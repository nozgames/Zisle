using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public class EffectComponentContext
    {
        private static LinkedList<EffectComponentContext> _pool = new LinkedList<EffectComponentContext>();

        private bool _enabled;

        public EffectContext EffectContext { get; private set; }
        public EffectComponent EffectComponent {get; private set; }
        public Effect Effect => EffectComponent.Effect;
        public Actor Target => EffectContext.Target;
        public Actor Source => EffectContext.Source;

        public object UserData { get; set; }

        public Tag Tag => EffectComponent.Tag;

        public LinkedListNode<EffectComponentContext> Node { get; private set; } 

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;

                if(_enabled)
                    EffectComponent.Apply(this);
                else
                    EffectComponent.Remove(this);
            }
        }

        public EffectComponentContext()
        {
            Node = new LinkedListNode<EffectComponentContext>(this);
        }

        public static EffectComponentContext Get (EffectContext effectContext, EffectComponent effectComponent)
        {
            EffectComponentContext componentContext;
            if (_pool.Count > 0)
            {
                componentContext = _pool.First.Value;
                _pool.Remove(componentContext.Node);
            }
            else
                componentContext = new EffectComponentContext();

            componentContext.EffectContext = effectContext;
            componentContext.EffectComponent = effectComponent;
            componentContext._enabled = false;

            return componentContext;
        }

        public void Release()
        {
            // Remove ourself from whatever list we are in
            if (Node.List != null)
                Node.List.Remove(Node);

            if(Enabled)
                EffectComponent.Remove(this);

            EffectComponent.Release(this);

            _enabled = false;
            UserData = null;

            _pool.AddLast(Node);
        }
    }
}
