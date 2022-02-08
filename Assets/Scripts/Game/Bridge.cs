using UnityEngine;

namespace NoZ.Zisle
{
    public class Bridge : Building
    {
        private Island _from;
        private Island _to;

        /// <summary>
        /// Bind the bridge to the given island so it can spawn the island when built (Host only)
        /// </summary>
        public void Bind (Island from, Island to)
        {
            _from = from;
            _to = to;
        }

        protected override void OnConstructedServer()
        {
            GameManager.Instance.Game.ShowIsland(_to);
        }

        protected override void OnConstructedClient()
        {
            base.OnConstructedClient();

            if (NavObstacle != null)
                NavObstacle.enabled = false;

            // TODO: spawn the next island..
        }
    }
}
