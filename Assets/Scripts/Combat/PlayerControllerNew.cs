using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerControllerNew : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float smoothing = 0.1f;

        private Rigidbody2D _rb;
        private Vector2 _currentVelocity;
        private Vector2 _smoothVelocity;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        private void FixedUpdate()
        {
            Vector2 input = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) input.y += 1;
                if (Keyboard.current.sKey.isPressed) input.y -= 1;
                if (Keyboard.current.aKey.isPressed) input.x -= 1;
                if (Keyboard.current.dKey.isPressed) input.x += 1;
            }

            if (input.sqrMagnitude > 1f) input.Normalize();

            Vector2 target = input * moveSpeed;
            _currentVelocity = Vector2.SmoothDamp(_currentVelocity, target, ref _smoothVelocity, smoothing);
            _rb.linearVelocity = _currentVelocity;
        }
    }
}