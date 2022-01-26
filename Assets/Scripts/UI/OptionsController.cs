using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class OptionsController : UIController
    {
        public new class UxmlFactory : UxmlFactory<OptionsController, UxmlTraits> { }

        public event Action onBack;

        public override void Initialize()
        {
            this.Q("back").AddManipulator(new Clickable(OnBack));
        }

        private void OnBack(EventBase e)
        {
            onBack?.Invoke();
        }
    }
}
