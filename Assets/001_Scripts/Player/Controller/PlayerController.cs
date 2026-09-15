using System;
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

        #endregion

        #region GroundCheck

        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        #endregion

        private Vector2 _moveVec;

        public void TakeDmg(float dmg)
        {
            playerHP -= dmg;

            if (playerHP <= 0)
            {
                Die();
            }
        }

        public Vector2 GetVector2()
            => _rb.position;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            PlayerManager.instance.SetPosition(transform.position);
            Debug.Log($"current Pos: {transform.position}");
        }

        public void Die()
        {
            PlayerState = PlayerState.Dead;
            GameManager.instance.StopGame();
        }

        private void FixedUpdate()
        {
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
            => _moveVec = ctx;

        public void Jump()
        {
            if (isGrounded)
            {
                _rb.AddForce(new Vector2(0f, playerJumpPower), ForceMode2D.Impulse);
            }
        }

        private void OnEnable()
        {
            InputManager.instance.Movement += Move;
            InputManager.instance.Jumping += Jump;
        }

        private void OnDisable()
        {
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