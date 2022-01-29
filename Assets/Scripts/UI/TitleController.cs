using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class TitleController : UIController
    {
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
            this.Q<Button>("solo-button").clicked += OnHost;
            this.Q<Button>("solo-button").Focus();
            this.Q<Button>("coop-button").clicked += OnJoin;
            this.Q<Button>("options-button").clicked += OnOptions;
            this.Q<Button>("quit-button").clicked += OnQuit;
        }

        private void OnHost()
        {
        }

        private void OnJoin()
        {

        }

        private void OnOptions() => UIManager.Instance.ShowOptions();

        private void OnQuit()
        {
            UIManager.Instance.ShowConfirmationPopup("Quit to desktop?", onYes: () =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            },
            onNo:() => UIManager.Instance.ShowTitle()
            );
        }
    }
}
