using _001_Scripts.Manager.Base;
using UnityEngine;

namespace _001_Scripts.Manager
{
    public class GameManager : SinManagerBase<GameManager>
    {
        private bool isArrived = false;

        [SerializeField] private Transform spawnPoint;

        public Transform GetSpawnPoint() => spawnPoint;
        public bool GetIsArrived() => isArrived;

        public void Arrive()
        {
            isArrived = true;
        }
    }
}
