using UnityEngine;

namespace Project.Juggling
{
    public class CrowdNPC : MonoBehaviour
    {
        private float _speed;
        private Transform _player;
        private float _slowdownFactor;
        private float _pushForce;
        private Rigidbody2D _rb;
        private float _swayOffset;
        private bool _initialized;

        public void Init(float speed, Transform player, float slowdownFactor, float pushForce)
        {
            _speed = speed;
            _player = player;
            _slowdownFactor = slowdownFactor;
            _pushForce = pushForce;
            _rb = GetComponent<Rigidbody2D>();
            _swayOffset = Random.Range(0f, Mathf.PI * 2);
            _initialized = true;
        }

        private void FixedUpdate()
        {
            if (!_initialized || _rb == null) return;

            // Бежит вверх через velocity (чтобы игрок мог расталкивать)
            float swayX = Mathf.Sin(Time.time * 2f + _swayOffset) * 0.5f;
            // Сохраняем X velocity (от толчков игрока), задаём Y вверх
            float currentX = _rb.linearVelocity.x;
            _rb.linearVelocity = new Vector2(currentX * 0.9f + swayX, _speed);

            if (transform.position.y > 20f)
                Destroy(gameObject);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!collision.collider.CompareTag("Player")) return;

            // Замедляем игрока пока толпа давит на него
            Rigidbody2D playerRb = collision.collider.GetComponentInParent<Rigidbody2D>();
            if (playerRb == null) playerRb = collision.collider.GetComponent<Rigidbody2D>();

            if (playerRb != null)
                playerRb.linearVelocity *= _slowdownFactor;
        }
    }
}
