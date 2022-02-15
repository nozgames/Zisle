using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

namespace NoZ.Zisle.UI
{
    public class UIDebugScreen : UIScreen
    {
        private Coroutine _coroutine = null;

        protected override void Awake()
        {
            base.Awake();

            Root.parent.pickingMode = PickingMode.Ignore;
            Root.pickingMode = PickingMode.Ignore;

        }

        protected override void OnShow()
        {
            base.OnShow();
            _coroutine = UIManager.Instance.StartCoroutine(UpdateValues());
        }

        protected override void OnHide()
        {
            base.OnHide();

            if(null != _coroutine)
                UIManager.Instance.StopCoroutine(_coroutine);
            _coroutine = null;
        }

        private IEnumerator UpdateValues()
        {
            do
            {
                yield return new WaitForFixedUpdate();

                UpdateServer();

                UpdatePlayer();
            }
            while (_coroutine != null);
        }

        public void UpdateServer()
        {
            Q<Label>("server-connection").text = ValueOrNone(GameManager.Instance.Connection);
            Q<Label>("server-joincode").text = ValueOrNone(GameManager.Instance.JoinCode);
        }

        private string ValueOrNone (string value, string none = "<None>") => (value == null || string.IsNullOrEmpty(value)) ? none : value;

        public void UpdatePlayer ()
        {
            var player = GameManager.Instance.LocalPlayer;
            if(player == null)
                return;

            var game = Game.Instance;
            if (null == game)
                return;

            var tile = game.WorldToTile(player.transform.position);
            var tileCell = TileGrid.WorldToCell(player.transform.position);
            var island = game.WorldToIsland(player.transform.position);
            var islandCell = IslandGrid.WorldToCell(player.transform.position);

            this.Q<Label>("player-tile").text = $"{tile} {tileCell}";
            this.Q<Label>("player-island").text = $"{IslandToString(island)} {islandCell}";

            var next = island.Next;
            if (next == null)
                this.Q<Label>("player-island-next").text = $"<None>";
            else
                this.Q<Label>("player-island-next").text = $"{IslandToString(next)} {next.Cell}";

            var path = Game.Instance.WorldToPathNode(player.transform.position);
            if(!path.IsPath)
                this.Q<Label>("player-tile-path").text = $"<None>";
            else
                this.Q<Label>("player-tile-path").text = $"{path.To}";

            //var test = island.WorldToCell(player.transform.position);
            //var test2 = TileGrid.WorldToCell(island.CellToWorld(test));
            //this.Q<Label>("player-tile-path").text = $"{test2}";
        }

        private string IslandToString (Island island)
        {
            if (island == null)
                return "<None>";

            return $"{island.Mesh.name} {CardinalDirectionToString(island.Rotation)}";
        }

        private string CardinalDirectionToString(CardinalDirection dir) => dir.ToString().Substring(0,1);
    }
}
