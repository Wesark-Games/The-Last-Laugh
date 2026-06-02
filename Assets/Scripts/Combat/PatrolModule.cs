using UnityEngine;

namespace Project.Combat
{
    public class PatrolModule : EnemyModule
    {
        [Header("[ ТОЧКИ ПАТРУЛЯ ]")]
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float patrolSpeed = 2f;
        [SerializeField] private float waitTime = 1f;
        [SerializeField] private float reachThreshold = 0.2f;

        private Rigidbody2D _rb;
        private Vector2 _currentTarget;
        private float _waitTimer;
        private bool _waiting;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (pointA != null) _currentTarget = pointA.position;
        }

        public override void UpdateModule() { }

        private void FixedUpdate()
        {
            if (Core == null) return;

            // Если враг увидел игрока — патруль останавливается (управление переходит к MovementModule)
            if (Core.Target != null) return;
            if (pointA == null || pointB == null) return;

            if (_waiting)
            {
                _rb.linearVelocity = Vector2.zero;
                _waitTimer -= Time.fixedDeltaTime;
                if (_waitTimer <= 0f) _waiting = false;
                return;
            }

            Vector2 pos = _rb.position;
            float dist = Vector2.Distance(pos, _currentTarget);

            if (dist <= reachThreshold)
            {
                // Достигли точки — ждём и меняем цель
                _waiting = true;
                _waitTimer = waitTime;
                _currentTarget = (_currentTarget == (Vector2)pointA.position)
                    ? pointB.position
                    : pointA.position;
            }
            else
            {
                Vector2 dir = (_currentTarget - pos).normalized;
                _rb.MovePosition(pos + dir * patrolSpeed * Time.fixedDeltaTime);
            }
        }

        private void OnDrawGizmos()
        {
            if (pointA != null && pointB != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(pointA.position, pointB.position);
                Gizmos.DrawWireSphere(pointA.position, 0.3f);
                Gizmos.DrawWireSphere(pointB.position, 0.3f);
            }
        }
    }
}
