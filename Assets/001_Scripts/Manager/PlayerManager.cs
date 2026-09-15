using System.Collections.Generic;
using _001_Scripts.Manager.Base;
using _001_Scripts.Player.Controller;
using _001_Scripts.Player.Interface;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Manager
{
    public sealed class PlayerManager : SinManagerBase<PlayerManager>
    {
        private Server server;
        private Info[] players;
        private List<Player.Player> player;

        [SerializeField] GameObject playerPrefab;

        protected override void Awake()
        {
            base.Awake();

            // LOL
            server = new Server();

            for (int i = 0; i < players.Length; i++)
            {
                player.Add(
                    Instantiate(playerPrefab, transform)
                        .AddComponent<Player.Player>()
                    );
            }
        }

        public void SetPosition(Vector2 position)
        {
            server.SetPos(new Info(position.x, position.y));
        }

        public Info[] GetPlayers()
            => players;

        private void Update()
        {
            for (int i = 0; i < players.Length; i++)
            {
                player[i].SetPos(
                    players[i]
                    );
            }
        }
    }
}