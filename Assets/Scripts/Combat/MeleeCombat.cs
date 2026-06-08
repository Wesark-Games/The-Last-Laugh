using UnityEngine;

namespace Project.Player
{
    public class MeleeCombat : MonoBehaviour
    {
        [SerializeField] private float attackRange    = 0.5f;
        [SerializeField] private float attackRadius   = 0.5f;
        [SerializeField] private int   attackDamage   = 10;
        [SerializeField] private float attackCooldown = 0.2f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private Transform characterTransform;

        private float _lastAttackTime;

        private void Awake()
        {
            if (characterTransform == null)
            {
                var rb = GetComponentInChildren<Rigidbody2D>();
                if (rb != null) characterTransform = rb.transform;
            }
        }

        public void TryAttack(Vector2 direction)
        {
            if (Time.time < _lastAttackTime + attackCooldown) return;
            if (characterTransform == null) return;
            _lastAttackTime = Time.time;

            Vector2 hitPos = (Vector2)characterTransform.position + direction.normalized * attackRange;
            ShowHitEffect(hitPos);

            var hits = Physics2D.OverlapCircleAll(hitPos, attackRadius, enemyLayer);
            Debug.Log("Удар! Найдено врагов: " + hits.Length);
            foreach (var hit in hits)
            {
                hit.GetComponent<Project.Combat.IDamageable>()?.TakeDamage(attackDamage);
            }

            Project.Combat.AlertSystem.Instance?.AlertNearbyEnemies(characterTransform.position, characterTransform);
        }

        private void ShowHitEffect(Vector2 pos)
        {
            GameObject effect = new GameObject("MeleeHit");
            effect.transform.position   = pos;
            effect.transform.localScale = new Vector3(attackRadius * 2, attackRadius * 2, 1f);

            SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
            sr.sprite       = GetComponentInChildren<SpriteRenderer>()?.sprite;
            sr.color        = new Color(1f, 1f, 0f, 0.6f);
            sr.sortingOrder = 50;

            Destroy(effect, 0.1f);
        }
    }
}