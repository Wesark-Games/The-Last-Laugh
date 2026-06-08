using UnityEngine;

namespace Project.Combat
{
    public class DetectionModule : EnemyModule
    {
        [Header("[ ЗРЕНИЕ ]")]
        [SerializeField] private float viewRadius = 6f;
        [Tooltip("Угол обзора в градусах (90 = широкое, 60 = узкое)")]
        [SerializeField] private float viewAngle = 90f;
        [SerializeField] private float losePlayerTime = 3f;

        [Header("[ СЛОИ ]")]
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask wallLayer;

        [Header("[ СПРАЙТ ]")]
        [Tooltip("Разворачивать спрайт по направлению движения")]
        [SerializeField] private bool flipSprite = true;

        private float _timeSinceLastSeen;
        private Vector2 _facingDir = Vector2.right;
        private Vector2 _lastPos;
        private SpriteRenderer _sprite;

        public override void Init(EnemyCore core)
        {
            base.Init(core);
            _lastPos = transform.position;
            _sprite = GetComponentInChildren<SpriteRenderer>();
        }

        public override void UpdateModule()
        {
            // Определяем направление по изменению позиции (работает с MovePosition)
            Vector2 currentPos = transform.position;
            Vector2 delta = currentPos - _lastPos;
            if (delta.sqrMagnitude > 0.0001f)
                _facingDir = delta.normalized;
            _lastPos = currentPos;

            // Если есть цель — смотрим на неё
            if (Core.Target != null)
                _facingDir = ((Vector2)Core.Target.position - currentPos).normalized;

            // Разворот спрайта
            if (flipSprite && _sprite != null && Mathf.Abs(_facingDir.x) > 0.01f)
                _sprite.flipX = _facingDir.x < 0;

            bool sees = CanSeePlayer(out Transform player);

            if (sees)
            {
                Core.SetTarget(player);
                Core.SetState(EnemyState.Chasing);
                _timeSinceLastSeen = 0f;
            }
            else if (Core.Target != null)
            {
                _timeSinceLastSeen += Time.deltaTime;
                if (_timeSinceLastSeen >= losePlayerTime)
                {
                    Core.SetTarget(null);
                    Core.SetState(EnemyState.Idle);
                    _timeSinceLastSeen = 0f;
                }
            }
        }

        private bool CanSeePlayer(out Transform player)
        {
            player = null;
            var hit = Physics2D.OverlapCircle(transform.position, viewRadius, playerLayer);
            if (hit == null) return false;

            Vector2 toPlayer = (Vector2)hit.transform.position - (Vector2)transform.position;
            float dist = toPlayer.magnitude;
            Vector2 dirToPlayer = toPlayer.normalized;

            // Проверка угла обзора
            float angle = Vector2.Angle(_facingDir, dirToPlayer);
            if (angle > viewAngle / 2f) return false;

            // Проверка стены между врагом и игроком
            if (wallLayer != 0)
            {
                RaycastHit2D wallHit = Physics2D.Raycast(transform.position, dirToPlayer, dist, wallLayer);
                if (wallHit.collider != null) return false;
            }

            player = hit.transform;
            return true;
        }

        public void ForceAlert(Transform target)
        {
            Core.SetTarget(target);
            Core.SetState(EnemyState.Chasing);
            _timeSinceLastSeen = 0f;
        }

        private void OnDrawGizmos()
        {
            Vector3 pos = transform.position;
            float half = viewAngle / 2f;

            Vector2 facing = Application.isPlaying ? _facingDir : Vector2.right;
            float baseAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;

            Vector3 leftDir  = DirFromAngle(baseAngle + half);
            Vector3 rightDir = DirFromAngle(baseAngle - half);

            Gizmos.color = new Color(1f, 1f, 0f, 0.8f);
            Gizmos.DrawLine(pos, pos + leftDir * viewRadius);
            Gizmos.DrawLine(pos, pos + rightDir * viewRadius);

            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            int segments = 20;
            Vector3 prev = pos + rightDir * viewRadius;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float a = baseAngle - half + viewAngle * t;
                Vector3 next = pos + DirFromAngle(a) * viewRadius;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }

        private Vector3 DirFromAngle(float angleDeg)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
        }
    }
}