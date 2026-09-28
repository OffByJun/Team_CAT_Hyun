using System.Collections.Generic;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Manager
{
    public sealed class RemotePlayerManager : MonoBehaviour
    {
        private readonly Dictionary<int, Player.Player> playerById = new();
        private readonly List<int> idsToRemove = new();
        private readonly Queue<Player.Player> playerPool = new();

        [SerializeField] private Player.Player playerPrefab;

        public void UpdatePlayers(Info[] players, int selfId)
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

                if (info.Id == selfId)
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
    }
}
