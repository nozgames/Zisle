using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIController : VisualElement
    {
        public virtual void Show() => parent.RemoveFromClassList("hidden");
        public virtual void Hide() => parent.AddToClassList("hidden");

        public virtual void Initialize () { }
    }
}
