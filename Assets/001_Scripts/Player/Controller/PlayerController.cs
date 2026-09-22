using System;
using System.Collections;
using System.IO;
using _001_Scripts.Manager;
using _001_Scripts.Player.Interface;
using _001_Scripts.Player.Type;
using UnityEngine;

namespace _001_Scripts.Player.Controller
{
    public sealed class PlayerController : GameBehaviour, IPlayer
    {
        private Rigidbody2D _rb;
        public PlayerState PlayerState { get; private set; }
        public MoveState MoveState { get; private set; }

        [SerializeField] private bool isGrounded = true;

        #region PlayerStat

        [SerializeField] private float playerSpeed = 5.0f;
        [SerializeField] private float playerJumpPower = 5.0f;
        [SerializeField] private float playerMaxHP = 100.0f;
        [SerializeField] private float playerHP = 100.0f;
        [SerializeField] private float respawnDelay = 3.0f;

        #endregion

        #region GroundCheck

        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        #endregion

        private Vector2 _moveVec;

        public void TakeDmg(float dmg)
        {
            if (PlayerState == PlayerState.Dead) return;

            playerHP -= dmg;

            if (playerHP <= 0)
            {
                Die();
            }
        }

        public void HeadHit(GameObject target)
        {
            // 블럭들이 뭐 해야하는지 내가 몰라서 일단 인터페이스만 두림
        }

        public Vector2 GetVector2()
            => _rb.position;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            InputManager.instance.Movement += Move;
            InputManager.instance.Jumping += Jump;

            PlayerManager.instance.SetPosition(transform.position);
            Debug.Log($"current Pos: {transform.position}");
        }

        public void Die()
        {
            PlayerState = PlayerState.Dead;
            _moveVec = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;

            GameManager.instance.StopGame();

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

            _rb.linearVelocity = Vector2.zero;

            GameManager.instance.StartGame();
        }

        private void FixedUpdate()
        {
            if (PlayerState == PlayerState.Dead) return;

            _rb.linearVelocity = new Vector2(
                _moveVec.x * playerSpeed,
                _rb.linearVelocity.y
            );

            UpdateMoveState();
            CheckGround();

            PlayerManager.instance.SetPosition(
                GetVector2()
            );
        }

        public void Move(Vector2 ctx)
        {
            if (PlayerState == PlayerState.Dead) return;
            _moveVec = ctx;
        }

        public void Jump()
        {
            if (PlayerState == PlayerState.Dead) return;

            if (isGrounded)
            {
                _rb.AddForce(new Vector2(0f, playerJumpPower), ForceMode2D.Impulse);
            }
        }

        private void OnDestroy()
        {
            if (InputManager.instance == null) return;

            InputManager.instance.Movement -= Move;
            InputManager.instance.Jumping -= Jump;
        }

        private void UpdateMoveState()
        {
            if (!isGrounded)
            {
                if (_rb.linearVelocity.y > 0.01f)
                {
                    MoveState = MoveState.Jump;
                }
                else
                {
                    MoveState = MoveState.Fall;
                }

                return;
            }

            if (Mathf.Abs(_rb.linearVelocity.x) > 0.01f)
            {
                MoveState = MoveState.Walk;
            }
            else
            {
                MoveState = MoveState.Idle;
            }
        }

        private void CheckGround()
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }
    }
}