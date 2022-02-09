using UnityEngine;

namespace NoZ.Zisle
{
    public class Bridge : Building
    {
        private Island _from;
        private Island _to;

        private Vector3 _spawnPosition;

        /// <summary>
        /// Bind the bridge to the given island so it can spawn the island when built (Host only)
        /// </summary>
        public void Bind (Island from, Island to)
        {
            _from = from;
            _to = to;

            _spawnPosition = transform.position + (_from.transform.position - _to.transform.position).normalized;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if(IsHost)
                Game.Instance.AddSpawnPoint(_to.Biome, _spawnPosition, Quaternion.LookRotation((_spawnPosition - _from.transform.position).normalized, Vector3.up));
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (IsHost)
                Game.Instance.RemoveSpawnPoint (_spawnPosition);
        }

        protected override void OnConstructedServer()
        {
            GameManager.Instance.Game.ShowIsland(_to);
            Game.Instance.RemoveSpawnPoint(_spawnPosition);
        }

        protected override void OnConstructedClient()
        {
            base.OnConstructedClient();

            if (NavObstacle != null)
                NavObstacle.enabled = false;
        }
    }
}
