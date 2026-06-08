using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Player
{
    public class WeaponVisual : MonoBehaviour
    {
        [SerializeField] private Transform characterTransform;
        [Tooltip("Расстояние от центра персонажа (в руках)")]
        [SerializeField] private float holdDistance = 0.5f;
        [SerializeField] private Camera gameCamera;
        [Tooltip("Включи если спрайт оружия нарисован дулом ВЛЕВО")]
        [SerializeField] private bool spriteFacesLeft = true;

        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
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

            Vector2 mousePos = gameCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 center = characterTransform.position;
            Vector2 dir = (mousePos - center).normalized;

            // Позиция в руках
            transform.position = center + dir * holdDistance;

            // Поворот к курсору
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (_sr != null)
            {
                // Если спрайт нарисован влево — отражаем по X чтобы смотрел на курсор
                _sr.flipX = spriteFacesLeft;
                // Чтобы не был вверх ногами когда целишься в левую половину
                _sr.flipY = Mathf.Abs(angle) > 90f;
            }
        }
    }
}