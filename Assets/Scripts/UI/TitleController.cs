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
            BindClick("solo-button", OnSolo).Focus();
            BindClick("coop-button", OnCooperative);
            BindClick("options-button", OnOptions);
            BindClick("quit-button", OnQuit);
        }

        private void OnSolo()
        {
#if false
            // TODO: check if there was a game in progress
            UIManager.Instance.ShowConfirmationPopup(
                "Resume previous solo game?", 
                yes: "Resume",
                no: "New Game",
                cancel: "Cancel",
                onYes: () =>
                {
                    // TODO: load old solo game
                    MultiplayerManager.Instance.HostLocal();
                    UIManager.Instance.ShowGame();
                }, 
                onNo: () => {
                    MultiplayerManager.Instance.HostLocal();
                    UIManager.Instance.ShowGame();
                    // TODO: start new solo game
                },
                onCancel: () =>
                {
                    UIManager.Instance.ShowTitle();
                }
                );
#else
            MultiplayerManager.Instance.HostLocal();
            UIManager.Instance.ShowGame();
#endif
        }

        private void OnCooperative()
        {
            UIManager.Instance.ShowCooperative();
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
