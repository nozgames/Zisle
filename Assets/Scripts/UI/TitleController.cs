using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class TitleController : UIController
    {
        public event Action onOptions;

        public new class UxmlFactory : UxmlFactory<TitleController, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
//            UxmlStringAttributeDescription m_StartScene = new UxmlStringAttributeDescription { name = "start-scene", defaultValue = "Main" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                //var sceneName = m_StartScene.GetValueFromBag(bag, cc);

                //((TitleController)ve).Init(sceneName);
            }
        }

        public override void Initialize()
        {
            this.Q("host").AddManipulator(new Clickable(OnHost));
            this.Q("join").AddManipulator(new Clickable(OnJoin));
            this.Q("options-button").AddManipulator(new Clickable(() => onOptions?.Invoke()));
            this.Q("quit-button").AddManipulator(new Clickable(OnQuit));

            Debug.Log("Geometry change");
        }

        private void OnHost(EventBase e)
        {
        }

        private void OnJoin(EventBase e)
        {

        }

        private void OnQuit(EventBase e)
        {
            UIManager.Instance.ShowConfirmationPopup("Quit", "Are you sure you want to quit?", onYes: () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
        }
    }
}
