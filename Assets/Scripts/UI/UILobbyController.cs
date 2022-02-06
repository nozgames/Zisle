using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle
{
    public class UILobbyController : UIController
    {
        public new class UxmlFactory : UxmlFactory<UILobbyController, UxmlTraits> { }

        public override bool BlurBackground => false;

        private List<ActorDefinition> _playerClasses;
        private ActorDefinition _playerClassLeft;
        private ActorDefinition _playerClassRight;

        public bool IsSolo { get; set; }

        public override void Initialize()
        {
            base.Initialize();

            _playerClasses = GameManager.Instance.ActorDefinitions.Where(d => d.ActorType == ActorType.Player).ToList();

            this.Q<Image>("player-left-preview").image = UIManager.Instance.PreviewLeftTexture;
            this.Q<Image>("player-right-preview").image = UIManager.Instance.PreviewRightTexture;

            this.Q<Button>("player-class-prev").clicked += () =>
            {
                var index = _playerClasses.IndexOf(_playerClassLeft);
                if (index == -1)
                    index = 0;
                else
                    index = (index + _playerClasses.Count - 1) % _playerClasses.Count;

                SetPlayerClassLeft(_playerClasses[index]);
            };

            this.Q<Button>("player-class-next").clicked += () =>
            {
                var index = _playerClasses.IndexOf(_playerClassLeft);
                if (index == -1)
                    index = 0;
                else
                    index = (index + 1) % _playerClasses.Count;

                SetPlayerClassLeft(_playerClasses[index]);
            };

            this.Q<Button>("ready").clicked += () =>
            {
                UIManager.Instance.StartGame(new GameOptions { MaxIslands = 4, PathWeight0 = 0.1f, PathWeight1 = 1.0f, PathWeight2 = 0.4f, PathWeight3 = 0.2f, StartingPaths = 1});
            };
        }

        public override void OnBeforeTransitionIn()
        {
            base.OnBeforeTransitionIn();

            SetPlayerClassLeft(_playerClasses[0]);

            if (IsSolo)
                this.Q("player-right").AddToClassList("hidden");
            else
            {
                this.Q("player-right").RemoveFromClassList("hidden");
                SetPlayerClassRight(_playerClasses[0]);
            }            
        }

        private void SetPlayerClassLeft(ActorDefinition def)
        {
            this.Q<Label>("player-left-name").text = def.DisplayName;
            UIManager.Instance.ShowLeftPreview(def);
            _playerClassLeft = def;
        }

        private void SetPlayerClassRight(ActorDefinition def)
        {
            this.Q<Label>("player-right-name").text = def.DisplayName;
            UIManager.Instance.ShowRightPreview(def);
            _playerClassRight = def;
        }
    }
}
