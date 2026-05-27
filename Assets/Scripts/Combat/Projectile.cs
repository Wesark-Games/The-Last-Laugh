using UnityEngine;

namespace Project.Combat
{
    public class Projectile : MonoBehaviour
    {
        private float _speed;
        private int _damage;
        private bool _isPlayerBullet;
        private Rigidbody2D _rb;

        public void Init(Vector2 direction, int damage, float speed, bool isPlayerBullet)
        {
            _rb = GetComponent<Rigidbody2D>();
            _damage = damage;
            _speed = speed;
            _isPlayerBullet = isPlayerBullet;
            _rb.linearVelocity = direction.normalized * _speed;
            Destroy(gameObject, 4f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Игнорируем источника выстрела
            if (_isPlayerBullet && other.CompareTag("Player")) return;
            if (!_isPlayerBullet && other.CompareTag("Enemy")) return;

            // Стена — уничтожаем пулю
            if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                Destroy(gameObject);
                return;
            }

            // Наносим урон
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
    }
}
