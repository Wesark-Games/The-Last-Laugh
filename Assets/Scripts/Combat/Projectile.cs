using UnityEngine;

namespace Project.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private LayerMask wallLayer;

        private Vector2 _direction;
        private float _damage;
        private float _speed;
        private bool _isPlayerBullet;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        public void Init(Vector2 direction, float damage, float speed, bool isPlayerBullet)
        {
            _direction = direction.normalized;
            _damage = damage;
            _speed = speed;
            _isPlayerBullet = isPlayerBullet;

            _rb.linearVelocity = _direction * _speed;

            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("ПУЛЯ ЗАДЕЛА: " + other.name + " | слой: " + LayerMask.LayerToName(other.gameObject.layer));

            if (_isPlayerBullet && other.CompareTag("Player")) return;
            if (!_isPlayerBullet && other.CompareTag("Enemy")) return;

            if (((1 << other.gameObject.layer) & wallLayer) != 0)
            {
                Destroy(gameObject);
                return;
            }

            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable == null) damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null) damageable = other.GetComponentInChildren<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(Mathf.RoundToInt(_damage));
                Destroy(gameObject);
            }
        }
    }
}