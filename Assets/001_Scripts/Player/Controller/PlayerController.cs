using System;
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

        #region PlayerStat

        [SerializeField] private float playerSpeed = 5.0f;
        [SerializeField] private float playerJumpPower = 5.0f;
        [SerializeField] private float playerMaxHP = 100.0f;
        [SerializeField] private float playerHP = 100.0f;

        #endregion

        private Vector2 moveVec;

        public void TryDmg(float dmg)
        {
            
        }

        public Vector2 GetPos()
            => _rb.position;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void OnDamage(float dmg)
        {
            playerHP -= dmg;

            if (playerHP <= 0)
                Die();
        }

        public void Die()
        {
            PlayerState = PlayerState.Dead;
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity =
                new Vector2(
                    moveVec.x *
                    playerSpeed,
                    _rb.linearVelocity.y);
        }

        public void Move(Vector2 ctx)
            => moveVec = ctx;

        public void Jump()
            => _rb.AddForce(new Vector2(0f, playerJumpPower), ForceMode2D.Impulse);

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
    }
}