using UnityEngine;

namespace Project.Juggling
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Torch : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 540f;
        [SerializeField] private float groundY = -3f;

        private Rigidbody2D _rb;
        private bool _hasFallen;
        private float _rotationDirection = 1f;

        public bool HasFallen => _hasFallen;
        public float CurrentY => transform.position.y;
        public Rigidbody2D Rb => _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (_hasFallen) return;

            // Постоянное вращение пока летит
            transform.Rotate(0f, 0f, rotationSpeed * _rotationDirection * Time.deltaTime);

            // Проверка падения на пол
            if (transform.position.y <= groundY)
            {
                _hasFallen = true;
                _rb.linearVelocity = Vector2.zero;
                _rb.gravityScale = 0f;
            }
        }

        public void Toss(Vector2 force)
        {
            if (_hasFallen) return;
            _rb.linearVelocity = force;
            _rotationDirection = force.x >= 0 ? 1f : -1f;
        }

        public void SetGroundY(float y) => groundY = y;
    }
}