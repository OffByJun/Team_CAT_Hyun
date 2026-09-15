using System;
using System.Collections.Generic;
using _001_Scripts.Manager.Base;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Manager
{
    public sealed class PlayerManager : SinManagerBase<PlayerManager>
    {
        private Server server;
        private Info[] players = Array.Empty<Info>();
        private readonly Dictionary<int, Player.Player> playerById = new Dictionary<int, Player.Player>();
        private readonly List<int> idsToRemove = new List<int>();
        private bool isConnected = true;

        [SerializeField] GameObject playerPrefab;

        protected override void Awake()
        {
            base.Awake();

            // LOL
            server = new Server();
            Debug.Log("Connected");

            TryFetchPlayers();
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
            SyncPlayers();
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

        private void SyncPlayers()
        {
            idsToRemove.Clear();
            foreach (int id in playerById.Keys)
            {
                idsToRemove.Add(id);
            }

            for (int i = 0; i < players.Length; i++)
            {
                Info info = players[i];

                if (!playerById.TryGetValue(info.Id, out Player.Player remotePlayer))
                {
                    remotePlayer = Instantiate(playerPrefab, transform).GetComponent<Player.Player>();
                    playerById.Add(info.Id, remotePlayer);
                }

                remotePlayer.SetPos(info);
                idsToRemove.Remove(info.Id);
            }

            for (int i = 0; i < idsToRemove.Count; i++)
            {
                int id = idsToRemove[i];
                Destroy(playerById[id].gameObject);
                playerById.Remove(id);
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
