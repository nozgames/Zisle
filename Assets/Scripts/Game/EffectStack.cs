using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public class EffectStack
    {
        public Effect Effect;
        public LinkedList<EffectContext> Contexts = new LinkedList<EffectContext>();
        public LinkedListNode<EffectStack> Node;

        public EffectStack()
        {
            Node = new LinkedListNode<EffectStack>(this);
        }
    }
}
