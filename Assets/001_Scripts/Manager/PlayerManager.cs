using System;
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
        private Info[] players = Array.Empty<Info>();
        private List<Player.Player> player = new List<Player.Player>();
        private bool isConnected = true;

        [SerializeField] GameObject playerPrefab;

        protected override void Awake()
        {
            base.Awake();

            // LOL
            server = new Server();
            Debug.Log("Connected");

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
            if (!isConnected) return;

            try
            {
                server.SetPos(new Info(position.x, position.y));
                // Debug.Log($"Current Player: {position.x}, {position.y}");
            }
            catch (InvalidOperationException e)
            {
                HandleConnectionLost(e);
            }
        }

        public Info[] GetPlayers()
            => players;

        private void Update()
        {
            TryFetchPlayers();

            for (int i = 0; i < players.Length; i++)
            {
                player[i - 1].SetPos(
                    players[i - 1]
                    );
            }
        }

        private void TryFetchPlayers()
        {
            if (!isConnected) return;

            try
            {
                players = server.GetPos();
            }
            catch (InvalidOperationException e)
            {
                HandleConnectionLost(e);
            }
        }

        private void HandleConnectionLost(InvalidOperationException e)
        {
            Debug.LogError($"서버 연결 끊김: {e.Message}");
            isConnected = false;
            server?.Dispose();
        }

        private void OnDestroy()
        {
            server?.Dispose();
        }
    }
}
