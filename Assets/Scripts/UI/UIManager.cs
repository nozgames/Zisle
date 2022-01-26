using System;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UIManager : Singleton<UIManager>
    {
        private VisualElement _root;

        private class UIController<T> where T : UIController
        {
            public static T instance { get; set; }
            public static T Bind(VisualElement element) => instance = element.Q<T>();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();            

            _root = GetComponent<UIDocument>().rootVisualElement;
            
            // Initialzie and bind all UIControllers
            _root.Query<UIController>().ForEach(c => c.Initialize());

            var title = UIController<TitleController>.Bind(_root);
            var options = UIController<OptionsController>.Bind(_root);
            var confirmPopup = UIController<ConfirmPopupController>.Bind(_root);

            title.Show();
            title.onOptions += () => { title.Hide(); options.Show(); };

            options.onBack += () => { options.Hide(); title.Show(); };
            options.Hide();

            confirmPopup.Hide();
        }

        public void ShowConfirmationPopup (string title, string message, string yes=null, string no=null, Action onYes=null, Action onNo=null)
        {
            UIController<ConfirmPopupController>.instance.Show(title, message, yes, no, onYes, onNo);
        }
    }
}
