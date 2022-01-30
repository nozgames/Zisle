using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIKeyboardControls : UIController
    {
        public new class UxmlFactory : UxmlFactory<UIKeyboardControls, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            BindClick("back", () => UIManager.Instance.ShowOptions());
        }
    }
}
