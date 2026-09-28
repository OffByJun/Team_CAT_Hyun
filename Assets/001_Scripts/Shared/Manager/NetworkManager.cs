using _001_Scripts.Manager.Base;
using _001_Scripts.Map;
using _001_Scripts.Player.Controller;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Manager
{
    public class NetworkManager : SinManagerBase<NetworkManager>, IServerHandler
    {
        public Server server;

        [SerializeField] private PlayerController player;

        private MapManager _mapManager;
        private RemotePlayerManager _remotePlayerManager;
        private bool _mapTransportConnected;
        private int playerId;

        protected override void Awake()
        {
            base.Awake();

            _mapManager = GetComponent<MapManager>();
            _remotePlayerManager = GetComponent<RemotePlayerManager>();
            server = new Server(this);
        }

        private void Update()
        {
            if (!_mapTransportConnected || server == null || player == null)
                return;

            Vector2 position = player.GetVector2();
            server.SetPos(new Info(position.x, position.y));
        }

        private void OnDestroy()
        {
            DisconnectMapTransport();
            server?.Dispose();
        }

        public void OnConnected(MoveRules rules)
        {
            playerId = server.MyId;
            player.SetRules(rules);
            player.SetMap(_mapManager);
            player.Respawn(_mapManager.GetSpawnFeetPosition());

            _mapTransportConnected = true;
            _mapManager.SetInteractionTransport(
                (x, y) =>
                {
                    if (!_mapTransportConnected || server == null) return false;
                    server.HitBlock(x, y);
                    return true;
                },
                id =>
                {
                    if (!_mapTransportConnected || server == null) return false;
                    server.KillMonster(id);
                    return true;
                });
        }

        public void OnDisconnected(string reason)
        {
            DisconnectMapTransport();
            Debug.Log($"접속이 종료되었습니다.\n사유: {reason}");
        }

        public void OnWorldReset()
        {
            _mapManager.ResetLocalInteractions();
            player.Respawn(_mapManager.GetSpawnFeetPosition());
        }

        public void OnMapUpdated(GridMap map)
        {
            _mapManager.OnMapUpdated(map);
        }

        public void OnPlayersUpdated(Info[] players)
        {
            _remotePlayerManager.UpdatePlayers(players, playerId);
        }

        public void OnMonstersUpdated(Monster[] monsters)
        {
            _mapManager.OnMonstersUpdated(monsters);
        }

        private void DisconnectMapTransport()
        {
            _mapTransportConnected = false;
            if (_mapManager != null)
                _mapManager.SetInteractionTransport(null, null);
        }
    }
}
