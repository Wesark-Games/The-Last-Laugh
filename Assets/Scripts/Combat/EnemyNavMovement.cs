using UnityEngine;

namespace Project.Combat
{
    public class EnemyNavMovement : EnemyModule
    {
        [Header("[ ДВИЖЕНИЕ ]")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float preferredDistance = 5f;
        [SerializeField] private float wallCheckDistance = 1.3f;
        [SerializeField] private LayerMask wallLayer;

        private Rigidbody2D _rb;
        private Animator _animator;
        private Vector2 _lastDir = Vector2.down;
        private Vector2 _prevPos;

        public override void Init(EnemyCore core)
        {
            base.Init(core);
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponentInChildren<Animator>();
            _prevPos = transform.position;
        }

        public override void UpdateModule() { }

        private void FixedUpdate()
        {
            if (Core == null || _rb == null) return;

            // Движение к цели
            if (Core.Target != null)
            {
                Vector2 pos = _rb.position;
                Vector2 toTarget = (Vector2)Core.Target.position - pos;
                if (toTarget.magnitude > preferredDistance)
                {
                    Vector2 moveDir = AvoidWalls(pos, toTarget.normalized);
                    _rb.MovePosition(pos + moveDir * moveSpeed * Time.fixedDeltaTime);
                }
            }

            // Анимация по РЕАЛЬНОМУ перемещению
            Vector2 cur = transform.position;
            Vector2 delta = cur - _prevPos;
            bool reallyMoving = delta.sqrMagnitude > 0.00001f;
            if (reallyMoving) _lastDir = delta.normalized;
            _prevPos = cur;

            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                _animator.SetBool("IsMoving", reallyMoving);
                _animator.SetFloat("DirX", _lastDir.x);
                _animator.SetFloat("DirY", _lastDir.y);
            }
        }

        private Vector2 AvoidWalls(Vector2 pos, Vector2 desired)
        {
            if (!Physics2D.Raycast(pos, desired, wallCheckDistance, wallLayer))
                return desired;
            for (int angle = 25; angle <= 90; angle += 25)
            {
                Vector2 left = Rotate(desired, angle);
                if (!Physics2D.Raycast(pos, left, wallCheckDistance, wallLayer)) return left;
                Vector2 right = Rotate(desired, -angle);
                if (!Physics2D.Raycast(pos, right, wallCheckDistance, wallLayer)) return right;
            }
            return desired;
        }

        private Vector2 Rotate(Vector2 v, float deg)
        {
            float r = deg * Mathf.Deg2Rad;
            float c = Mathf.Cos(r), s = Mathf.Sin(r);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }
    }
}