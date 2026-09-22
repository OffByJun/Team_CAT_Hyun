using System;
using System.Collections.Generic;
using _001_Scripts.Manager.Base;
using _001_Scripts.Player.Controller;
using School.PositionSync;
using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

namespace _001_Scripts.Manager
{
    public class NetworkManager : SinManagerBase<NetworkManager>, IServerHandler
    {
        public Server server;
        private readonly Dictionary<int, Player.Player> playerById = new Dictionary<int, Player.Player>();
        private readonly List<int> idsToRemove = new();

        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private PlayerController player;
        private int playerId;

        private void Awake()
        {
            base.Awake();
            server = new Server(this);
            playerId = server.MyId;

            Debug.Log("Connected");
            Debug.Log($"serverId: {server.MyId}");
        }

        public void OnWorldReset()
        {
            Debug.Log("World Reset");
            player.SetPos(new Vector2(0, 0));
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

        public void OnMapUpdated(GridMap map)
        {
        }

        public void OnPlayersUpdated(Info[] players)
        {
            idsToRemove.Clear();

            foreach (int id in playerById.Keys)
            {
                idsToRemove.Add(id);
            }

            for (int i = 0; i < players.Length; i++)
            {
                Info info = players[i];

                if (!playerById.TryGetValue(info.Id, out Player.Player player))
                {
                    player = Instantiate(playerPrefab, transform)
                        .GetComponent<Player.Player>();

                    playerById.Add(info.Id, player);
                }

                player.SetPos(info);

                idsToRemove.Remove(info.Id);
            }

            for (int i = 0; i < idsToRemove.Count; i++)
            {
                int id = idsToRemove[i];

                Destroy(playerById[id].gameObject);
                playerById.Remove(id);
            }
        }

        public void OnMonstersUpdated(Monster[] monsters)
        {
            
        }
    }
}