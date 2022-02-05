    using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NoZ.Zisle
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private Camera _camera = null;
        [SerializeField] private Biome[] _biomes = null;

        public Camera Camera => _camera;

        public override void Initialize()
        {
            base.Initialize();

            InputManager.Instance.onPlayerMenu += OnPlayerMenu;

            foreach (var biome in _biomes)
                biome.RegisterNetworkId();

#if false
            SpawnIsland(0, 0, 1);

            for(int i=0; i<20; i++)
            {
                var cells = GetOpenConnectableCells();
                if (cells.Count > 0)
                {
                    var cell = cells[Random.Range(0, cells.Count)];
                    SpawnIsland(cell.x, cell.y, i + 2);
                }
            }
#endif
        }

        private void OnPlayerMenu()
        {
            UIManager.Instance.ShowGameMenu();
        }

        public void StopGame()
        {
            MultiplayerManager.Instance.LeaveGame();

            UIManager.Instance.ShowTitle();
        }

        public void Resume ()
        {
            InputManager.Instance.EnableMenuActions(false);
            InputManager.Instance.EnablePlayerActions(true);
        }

        public void Pause ()
        {
            InputManager.Instance.EnableMenuActions(true);
            InputManager.Instance.EnablePlayerActions(false);
        }

#if false
        /// <summary>
        /// Choose an island to spawn using the given level.  The home island is considered level 1.
        /// </summary>
        /// <param name="level">Level to spawn island at</param>
        /// <returns>Island</returns>
        public Island ChooseIsland (int level)
        {
            // TODO: need a way to figure out the connection points

        }
#endif




    }
}

