using System;
using System.Collections.Generic;
using School.PositionSync;

namespace _001_Scripts.Map
{
    // Unity 오브젝트와 분리한 중복 요청/수집 상태. 맵 재배치에도 유지합니다.
    internal sealed class MapInteractionState
    {
        private readonly Dictionary<int, TileKind> pendingBlocks = new();
        private readonly HashSet<int> collectedCoins = new();
        private readonly HashSet<int> pendingMonsters = new();
        private string mapId;

        public bool IsCollected(int id) => collectedCoins.Contains(id);

        public void ApplyMap(GridMap map)
        {
            if (mapId != map.Id)
            {
                ResetLocalProgress();
                mapId = map.Id;
            }
            var completed = new List<int>();
            foreach (var entry in pendingBlocks)
            {
                int x = entry.Key % map.Width;
                int y = entry.Key / map.Width;
                if (y >= map.Height || map.GetTile(x, y) != entry.Value)
                    completed.Add(entry.Key);
            }
            foreach (int id in completed) pendingBlocks.Remove(id);
        }

        public bool TryHit(GridCell cell, Func<int, int, bool> send, out BlockContent content)
        {
            content = BlockContent.None;
            if (send == null || pendingBlocks.ContainsKey(cell.Id)) return false;
            if (cell.Kind != TileKind.Brick && cell.Kind != TileKind.Question &&
                cell.Kind != TileKind.HiddenOneUpBlock) return false;
            // 콜백 중 재진입하더라도 중복 전송하지 않습니다.
            pendingBlocks.Add(cell.Id, cell.Kind);
            try
            {
                if (!send(cell.X, cell.Y))
                {
                    pendingBlocks.Remove(cell.Id);
                    return false;
                }
            }
            catch
            {
                pendingBlocks.Remove(cell.Id);
                throw;
            }
            content = cell.Content;
            return true;
        }

        public bool TryCollect(GridCell cell, out BlockContent content)
        {
            content = BlockContent.None;
            if (cell.Kind != TileKind.Coin || !collectedCoins.Add(cell.Id)) return false;
            content = BlockContent.Coin;
            return true;
        }

        public bool TryStomp(int id, Func<int, bool> send)
        {
            if (send == null || !pendingMonsters.Add(id)) return false;
            try
            {
                if (send(id)) return true;
            }
            catch
            {
                pendingMonsters.Remove(id);
                throw;
            }
            pendingMonsters.Remove(id);
            return false;
        }

        public void RemoveMonster(int id) => pendingMonsters.Remove(id);

        public void ClearPendingRequests()
        {
            pendingBlocks.Clear();
            pendingMonsters.Clear();
        }

        public void ResetLocalProgress()
        {
            ClearPendingRequests();
            collectedCoins.Clear();
        }
    }
}
