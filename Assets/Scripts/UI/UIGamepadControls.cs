using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIGamepadControls : UIController
    {
        public new class UxmlFactory : UxmlFactory<UIGamepadControls, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            BindClick("back", () => UIManager.Instance.ShowOptions());
        }
    }
}
