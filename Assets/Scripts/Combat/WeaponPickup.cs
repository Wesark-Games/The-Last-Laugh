using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Combat
{
    public class WeaponPickup : MonoBehaviour
    {
        [Header("[ ОРУЖИЕ ]")]
        [SerializeField] private WeaponData weaponData;
        [Tooltip("Иконка для HUD (опционально)")]
        [SerializeField] private Sprite weaponIcon;

        [Header("[ АНИМАЦИЯ ]")]
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.15f;

        private Vector3 _startPos;
        private Project.Player.WeaponInventory _playerInRange;

        private void Start()
        {
            _startPos = transform.position;
        }

        private void Update()
        {
            float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(_startPos.x, newY, _startPos.z);

            if (_playerInRange != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                Debug.Log("E нажата, игрок рядом. weaponData: " + (weaponData != null));
                bool picked = _playerInRange.PickupWeapon(weaponData, weaponIcon);
                Debug.Log("Результат подбора: " + picked);
                if (picked)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("В зону оружия вошёл: " + other.name + " | тег: " + other.tag);
            if (!other.CompareTag("Player")) return;
            var inventory = other.GetComponentInParent<Project.Player.WeaponInventory>();
            if (inventory != null)
            {
                _playerInRange = inventory;
                Debug.Log("Игрок в зоне подбора оружия!");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            var inventory = other.GetComponentInParent<Project.Player.WeaponInventory>();
            if (inventory != null && inventory == _playerInRange) _playerInRange = null;
        }
    }
}