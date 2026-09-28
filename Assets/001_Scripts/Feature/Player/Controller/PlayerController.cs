using System.Collections;
using _001_Scripts.Manager;
using _001_Scripts.Map;
using _001_Scripts.Map.Tiles.Interface;
using _001_Scripts.Player.Interface;
using _001_Scripts.Player.Type;
using School.PositionSync;
using UnityEngine;

namespace _001_Scripts.Player.Controller
{
    public sealed class PlayerController : GameBehaviour, IPlayer
    {
        private MovementController _movementController;
        private Rigidbody2D _rb;
        private Collider2D _bodyCollider;
        private MapManager _mapManager;
        private Bounds _previousBounds;
        private float _previousVerticalVelocity;
        private float _invincibleUntil;
        private bool _hasPhysicsSnapshot;

        public PlayerState PlayerState { get; private set; }
        public MoveState MoveState => _movementController.MoveState;

        #region PlayerStat

        [SerializeField] private float playerMaxHP = 100.0f;
        [SerializeField] private float playerHP = 100.0f;
        [SerializeField] private float respawnDelay = 3.0f;
        [SerializeField] private float enemyDamage = 1.0f;
        [SerializeField] private float stompBouncePower = 5.0f;
        [SerializeField] private float invincibleDuration = 1.0f;
        [SerializeField] private int coinScore = 100;

        #endregion

        private void Awake()
        {
            _movementController = GetComponent<MovementController>();
            _rb = GetComponent<Rigidbody2D>();
            _bodyCollider = GetComponent<Collider2D>();
        }

        private void Start()
        {
            InputManager.instance.Movement += Move;
            InputManager.instance.Jumping += Jump;
            SavePhysicsSnapshot();
        }

        private void Update()
        {
            if (PlayerState == PlayerState.Dead || _mapManager == null || _bodyCollider == null)
                return;

            if (_bodyCollider.bounds.min.y < _mapManager.GetFallBoundaryY())
                Die();
        }

        private void FixedUpdate()
        {
            SavePhysicsSnapshot();
        }

        private void OnDestroy()
        {
            if (InputManager.instance == null) return;

            InputManager.instance.Movement -= Move;
            InputManager.instance.Jumping -= Jump;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (PlayerState == PlayerState.Dead || !WasMovingUp()) return;

            for (int i = 0; i < collision.contactCount; i++)
            {
                if (collision.GetContact(i).normal.y > -0.5f) continue;
                HeadHit(collision.gameObject);
                return;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (PlayerState == PlayerState.Dead) return;

            ICollectable collectible = other.GetComponentInParent<ICollectable>();
            if (collectible != null && collectible.TryCollect(out BlockContent content))
            {
                if (content == BlockContent.Coin)
                {
                    ScoreManager.instance.AddCoin();
                    ScoreManager.instance.AddScore(coinScore);
                }
                return;
            }

            IStompable enemy = other.GetComponentInParent<IStompable>();
            if (enemy != null)
            {
                HandleEnemyContact(enemy, other.bounds);
                return;
            }

            IBreakable block = other.GetComponentInParent<IBreakable>();
            if (block != null && IsHeadHitFromBelow(other.bounds))
                HeadHit(other.gameObject);
        }

        public void TakeDmg(float dmg)
        {
            if (PlayerState == PlayerState.Dead || Time.time < _invincibleUntil) return;

            playerHP -= dmg;
            _invincibleUntil = Time.time + invincibleDuration;

            if (playerHP <= 0) Die();
        }

        public void HeadHit(GameObject target)
        {
            if (PlayerState == PlayerState.Dead || target == null) return;

            IBreakable block = target.GetComponentInParent<IBreakable>();
            block?.TryHit(out BlockContent _);
        }

        public Vector2 GetVector2()
            => _movementController.Position;

        public void SetPos(Vector2 pos)
            => transform.position = pos;

        public void SetRules(MoveRules rules)
            => _movementController.SetRules(rules);

        public void SetMap(MapManager mapManager)
            => _mapManager = mapManager;

        public void Die()
        {
            PlayerState = PlayerState.Dead;
            _movementController.Stop();

            StartCoroutine(RespawnRoutine());
        }

        public void Respawn()
        {
            if (_mapManager != null)
            {
                Respawn(_mapManager.GetSpawnFeetPosition());
                return;
            }

            playerHP = playerMaxHP;
            PlayerState = PlayerState.Alive;
            _invincibleUntil = 0f;

            Transform spawnPoint = GameManager.instance.GetSpawnPoint();
            if (spawnPoint != null)
                transform.position = spawnPoint.position;

            _movementController.Stop();
            SavePhysicsSnapshot();
        }

        public void Respawn(Vector3 feetPosition)
        {
            playerHP = playerMaxHP;
            PlayerState = PlayerState.Alive;
            _invincibleUntil = 0f;

            float feetOffset = _bodyCollider != null
                ? transform.position.y - _bodyCollider.bounds.min.y
                : 0f;
            transform.position = feetPosition + Vector3.up * feetOffset;
            _movementController.Stop();
            SavePhysicsSnapshot();
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

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);
            Respawn();
        }

        private void HandleEnemyContact(IStompable enemy, Bounds enemyBounds)
        {
            bool stompFromAbove = _hasPhysicsSnapshot && _previousVerticalVelocity <= 0f &&
                _previousBounds.min.y >= enemyBounds.max.y - 0.08f;

            if (stompFromAbove)
            {
                if (enemy.TryStomp())
                    _movementController.Bounce(stompBouncePower);
                return;
            }

            TakeDmg(enemyDamage);
        }

        private bool IsHeadHitFromBelow(Bounds targetBounds)
        {
            if (!_hasPhysicsSnapshot || !WasMovingUp()) return false;

            bool wasBelow = _previousBounds.max.y <= targetBounds.min.y + 0.08f;
            bool overlapsHorizontally = _previousBounds.max.x > targetBounds.min.x &&
                _previousBounds.min.x < targetBounds.max.x;
            return wasBelow && overlapsHorizontally;
        }

        private bool WasMovingUp()
            => _previousVerticalVelocity > 0.01f || _movementController.VerticalVelocity > 0.01f;

        private void SavePhysicsSnapshot()
        {
            if (_bodyCollider == null || _rb == null) return;
            _previousBounds = _bodyCollider.bounds;
            _previousVerticalVelocity = _rb.linearVelocity.y;
            _hasPhysicsSnapshot = true;
        }
    }
}
