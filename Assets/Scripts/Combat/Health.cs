using System;
using System.Collections;
using UnityEngine;

namespace Project.Combat
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 100;
        private int _currentHealth;
        private SpriteRenderer _sprite;

        public event Action<int, int> OnHealthChanged;
        public event Action OnDeath;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;

        private void Awake()
        {
            _currentHealth = maxHealth;
            _sprite = GetComponentInChildren<SpriteRenderer>();
        }

        public void TakeDamage(int damage)
        {
            if (_currentHealth <= 0) return;
            _currentHealth = Mathf.Max(0, _currentHealth - damage);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            if (_sprite != null) StartCoroutine(FlashRed());
            if (_currentHealth == 0)
            {
                OnDeath?.Invoke();
                if (gameObject.CompareTag("Enemy"))
                    ScoreManager.Instance?.AddScore(100);
                Destroy(gameObject);
            }
        }

        public void Heal(int amount)
        {
            _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        private IEnumerator FlashRed()
        {
            if (_sprite == null) yield break;
            Color original = _sprite.color;
            _sprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            _sprite.color = original;
        }
    }
}
