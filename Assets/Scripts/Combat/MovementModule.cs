using UnityEngine;

namespace Project.Combat
{
    public class MovementModule : EnemyModule
    {
        [SerializeField] private float speed       = 3f;
        [SerializeField] private float attackRange = 0.8f;
        [SerializeField] private float attackDamage  = 10f;
        [SerializeField] private float attackCooldown = 1f;

        private float _lastAttackTime;
        private Rigidbody2D _rb;

        private void Awake() => _rb = GetComponent<Rigidbody2D>();

        public override void UpdateModule() { }

        private void FixedUpdate()
        {
            if (Core == null || Core.Target == null) return;

            float dist = Vector2.Distance(transform.position, Core.Target.position);

            if (dist > attackRange)
            {
                Core.SetState(EnemyState.Chasing);
                Vector2 dir = (Core.Target.position - transform.position).normalized;
                _rb.MovePosition(_rb.position + dir * speed * Time.fixedDeltaTime);
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
                Core.SetState(EnemyState.Attacking);

                if (Time.time >= _lastAttackTime + attackCooldown)
                {
                    _lastAttackTime = Time.time;
                    Core.Target.GetComponent<IDamageable>()?.TakeDamage((int)attackDamage);
                    SpawnHitEffect();
                }
            }
        }

        private void SpawnHitEffect()
        {
            if (Core.Target == null) return;
            Vector2 dir      = (Core.Target.position - transform.position).normalized;
            Vector3 spawnPos = transform.position + (Vector3)(dir * 0.5f);

            GameObject effect = new GameObject("HitEffect");
            effect.transform.position   = spawnPos;
            effect.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

            SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
            sr.sprite       = GetComponentInChildren<SpriteRenderer>()?.sprite;
            sr.color        = new Color(1f, 0.3f, 0f, 0.8f);
            sr.sortingOrder = 10;

            Destroy(effect, 0.12f);
        }
    }
}
