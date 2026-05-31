using UnityEngine;

namespace Project.Combat
{
    public class WeaponPickup : MonoBehaviour
    {
        [Header("[ ОРУЖИЕ ]")]
        [SerializeField] private WeaponData weaponData;
        [SerializeField] private int slotIndex = 1;
        [Tooltip("Спрайт оружия на земле (опционально)")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("[ АНИМАЦИЯ ]")]
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.15f;

        private Vector3 _startPos;

        private void Start()
        {
            _startPos = transform.position;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            // Покачивание вверх-вниз
            float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(_startPos.x, newY, _startPos.z);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            // Ищем WeaponInventory у игрока
            var inventory = other.GetComponentInParent<Project.Player.WeaponInventory>();
            if (inventory == null) return;

            // Кладём оружие в слот
            bool picked = inventory.PickupWeapon(slotIndex, weaponData);
            if (picked)
            {
                Debug.Log("Подобрано оружие: " + weaponData.name);
                Destroy(gameObject);
            }
        }
    }
}
