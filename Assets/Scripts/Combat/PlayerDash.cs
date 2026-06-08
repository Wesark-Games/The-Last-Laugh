using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Player
{
    public class PlayerDash : MonoBehaviour
    {
        [SerializeField] private float dashSpeed    = 14f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float dashCooldown = 1f;

        private Rigidbody2D _rb;
        private PlayerController _controller;
        private float _lastDashTime;
        private bool _isDashing;

        private void Awake()
        {
            _rb = GetComponentInParent<Rigidbody2D>();
            _controller = GetComponentInParent<PlayerController>();
        }

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                TryDash();
        }

        private void TryDash()
        {
            if (_isDashing || Time.time < _lastDashTime + dashCooldown) return;

            Vector2 dir = _rb.linearVelocity.normalized;
            if (dir == Vector2.zero) dir = Vector2.up;

            StartCoroutine(DoDash(dir));
        }

        private System.Collections.IEnumerator DoDash(Vector2 dir)
        {
            _isDashing = true;
            _lastDashTime = Time.time;

            // Отключаем движение игрока на время рывка
            _controller?.SetMovementEnabled(false);

            float timer = 0f;
            while (timer < dashDuration)
            {
                _rb.linearVelocity = dir * dashSpeed;
                timer += Time.deltaTime;
                yield return null;
            }

            _controller?.SetMovementEnabled(true);
            _isDashing = false;
        }

        public bool IsDashing => _isDashing;
    }
}