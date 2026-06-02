using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Player
{
    public class WeaponVisual : MonoBehaviour
    {
        [SerializeField] private Transform characterTransform;
        [SerializeField] private Vector3 holdOffset = new Vector3(0.3f, 0f, 0f);
        [SerializeField] private Camera gameCamera;

        private void Awake()
        {
            if (gameCamera == null) gameCamera = Camera.main;
            if (characterTransform == null)
            {
                var rb = GetComponentInParent<Rigidbody2D>();
                if (rb != null) characterTransform = rb.transform;
            }
        }

        private void Update()
        {
            if (characterTransform == null || gameCamera == null) return;

            // Позиция: у персонажа
            transform.position = characterTransform.position;

            // Направление к курсору
            Vector2 mousePos = gameCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 dir = (mousePos - (Vector2)characterTransform.position).normalized;

            // Поворот к курсору
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // Отражение по Y если смотрим влево
            Vector3 scale = transform.localScale;
            if (Mathf.Abs(angle) > 90f)
            {
                scale.y = -Mathf.Abs(scale.y);
            }
            else
            {
                scale.y = Mathf.Abs(scale.y);
            }
            transform.localScale = scale;
        }
    }
}