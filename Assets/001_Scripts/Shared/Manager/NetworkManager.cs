using _001_Scripts.Manager.Base;
using _001_Scripts.Player.Controller;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Manager
{
    public class NetworkManager : SinManagerBase<NetworkManager>, IServerHandler
    {
        public Server server;

        [SerializeField] private PlayerController player;

        private RemotePlayerManager _remotePlayerManager;
        private int playerId;

        private void Awake()
        {
            base.Awake();

            _remotePlayerManager = GetComponent<RemotePlayerManager>();

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
            _remotePlayerManager.UpdatePlayers(players, playerId);
        }

        public void OnMonstersUpdated(Monster[] monsters)
        {
        }
    }
}
