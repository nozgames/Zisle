using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoZ.Zisle
{
    public class UIClickBlocker : UIScreen
    {
        protected override void Awake()
        {
            base.Awake();

            Root.pickingMode = UnityEngine.UIElements.PickingMode.Position;
        }
    }
}
