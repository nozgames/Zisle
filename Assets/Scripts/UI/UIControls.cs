using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIControls : UIController
    {
        public new class UxmlFactory : UxmlFactory<UIControls, UxmlTraits> { }

        public override void Initialize()
        {
            base.Initialize();

            BindClick("back", () => UIManager.Instance.ShowOptions());
        }
    }
}
