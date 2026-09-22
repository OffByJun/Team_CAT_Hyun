using System.Collections.Generic;
using _001_Scripts.Manager.Base;
using _001_Scripts.Player.Controller;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Manager
{
    public class NetworkManager : SinManagerBase<NetworkManager>, IServerHandler
    {
        public Server server;

        private readonly Dictionary<int, Player.Player> playerById = new();
        private readonly List<int> idsToRemove = new();
        private readonly Queue<Player.Player> playerPool = new();

        [SerializeField] private Player.Player playerPrefab;
        [SerializeField] private PlayerController player;

        private int playerId;

        private void Awake()
        {
            base.Awake();

            server = new Server(this);
            playerId = server.MyId;

            Debug.Log("Connected");
            Debug.Log($"serverId: {playerId}");
        }

        private void OnDestroy()
        {
            server?.Dispose();
        }

        public void OnConnected(MoveRules rules)
        {
            player.SetRules(rules);
        }

        public void OnDisconnected(string reason)
        {
            Debug.Log($"접속이 종료되었습니다\n사유: {reason}");
        }

        public void OnWorldReset()
        {
            SceneManager.instance.LoadScene("main");
        }

        public void OnMapUpdated(GridMap map)
        {
        }

        public void OnPlayersUpdated(Info[] players)
        {
            if (players == null)
                return;

            idsToRemove.Clear();

            foreach (int id in playerById.Keys)
            {
                idsToRemove.Add(id);
            }

            for (int i = 0; i < players.Length; i++)
            {
                Info info = players[i];

                // 나는 이미 씬에 존재하니까 생성 안 함
                if (info.Id == playerId)
                    continue;

                if (!playerById.TryGetValue(info.Id, out Player.Player otherPlayer))
                {
                    otherPlayer = GetPlayer();

                    playerById.Add(info.Id, otherPlayer);
                }

                otherPlayer.SetPos(info);

                idsToRemove.Remove(info.Id);
            }

            for (int i = 0; i < idsToRemove.Count; i++)
            {
                int id = idsToRemove[i];

                if (!playerById.TryGetValue(id, out Player.Player otherPlayer))
                    continue;

                playerById.Remove(id);

                ReleasePlayer(otherPlayer);
            }
        }

        private Player.Player GetPlayer()
        {
            if (playerPool.Count > 0)
            {
                Player.Player otherPlayer = playerPool.Dequeue();

                otherPlayer.gameObject.SetActive(true);

                return otherPlayer;
            }

            return Instantiate(playerPrefab, transform);
        }

        private void ReleasePlayer(Player.Player otherPlayer)
        {
            if (otherPlayer == null)
                return;

            otherPlayer.gameObject.SetActive(false);

            playerPool.Enqueue(otherPlayer);
        }

        public void OnMonstersUpdated(Monster[] monsters)
        {
        }
    }
}