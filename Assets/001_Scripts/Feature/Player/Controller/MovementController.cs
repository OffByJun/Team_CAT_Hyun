using _001_Scripts.Player.Type;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Player.Controller
{
    public class MovementController : GameBehaviour
    {
        private Rigidbody2D _rb;

        [SerializeField] private float playerSpeed = 5.0f;
        [SerializeField] private float playerJumpPower = 5.0f;

        [SerializeField] private bool isGrounded = true;

        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask groundLayer;

        public MoveState MoveState { get; private set; }
        public Vector2 Position => _rb.position;
        private Vector2 _moveVec;

        private void Awake()
            => _rb = GetComponent<Rigidbody2D>();

        public void SetRules(MoveRules rules)
        {
            playerSpeed = rules.Speed;
            playerJumpPower = rules.JumpPower;
            Physics2D.gravity = new Vector2(0, -rules.Gravity);
        }

        private void FixedUpdate()
        {
            _rb.linearVelocity = new Vector2(
                _moveVec.x * playerSpeed,
                _rb.linearVelocity.y
            );

            UpdateMoveState();
            CheckGround();
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

        public void Stop()
        {
            _moveVec = Vector2.zero;
            _rb.linearVelocity = Vector2.zero;
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
