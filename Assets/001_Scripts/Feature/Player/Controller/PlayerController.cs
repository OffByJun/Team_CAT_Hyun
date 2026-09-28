using System;
using System.Collections;
using System.IO;
using _001_Scripts.Manager;
using _001_Scripts.Player.Interface;
using _001_Scripts.Player.Type;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Player.Controller
{
    public sealed class PlayerController : GameBehaviour, IPlayer
    {
        private MovementController _movementController;
        public PlayerState PlayerState { get; private set; }
        public MoveState MoveState => _movementController.MoveState;

        #region PlayerStat

        [SerializeField] private float playerMaxHP = 100.0f;
        [SerializeField] private float playerHP = 100.0f;
        [SerializeField] private float respawnDelay = 3.0f;

        #endregion

        public void TakeDmg(float dmg)
        {
            if (PlayerState == PlayerState.Dead) return;

            playerHP -= dmg;

            if (playerHP <= 0) Die();
        }

        public void HeadHit(GameObject target)
        {
            // 블럭들이 뭐 해야하는지 내가 몰라서 일단 인터페이스만 두림
        }

        public Vector2 GetVector2()
            => _movementController.Position;

        public void SetPos(Vector2 pos)
            => transform.position = pos;

        public void SetRules(MoveRules rules)
            => _movementController.SetRules(rules);

        private void Awake()
            => _movementController = GetComponent<MovementController>();

        private void Start()
        {
            InputManager.instance.Movement += Move;
            InputManager.instance.Jumping += Jump;

            NetworkManager.instance.server.SetPos(
                new Info(
                    transform.position.x, transform.position.y));
            Debug.Log($"current Pos: {transform.position}");
        }

        public void Die()
        {
            PlayerState = PlayerState.Dead;
            _movementController.Stop();

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);
            Respawn();
        }

        public void Respawn()
        {
            playerHP = playerMaxHP;
            PlayerState = PlayerState.Alive;

            Transform spawnPoint = GameManager.instance.GetSpawnPoint();
            if (spawnPoint != null)
            {
                transform.position = spawnPoint.position;
            }

            _movementController.Stop();
        }

        public void Move(Vector2 ctx)
        {
            if (PlayerState == PlayerState.Dead) return;
            _movementController.Move(ctx);
        }

        public void Jump()
        {
            if (PlayerState == PlayerState.Dead) return;
            _movementController.Jump();
        }

        private void OnDestroy()
        {
            if (InputManager.instance == null) return;

            InputManager.instance.Movement -= Move;
            InputManager.instance.Jumping -= Jump;
        }
    }
}
