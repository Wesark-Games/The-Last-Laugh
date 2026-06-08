using UnityEngine;

namespace Project.Combat
{
    public class AmmoPickup : MonoBehaviour
    {
        [SerializeField] private int ammoAmount = 12;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.15f;

        private Vector3 _startPos;

        private void Start() { _startPos = transform.position; }

        private void Update()
        {
            float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(_startPos.x, newY, _startPos.z);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            var inventory = other.GetComponentInParent<Project.Player.WeaponInventory>();
            if (inventory == null || !inventory.HasRangedWeapon) return;

            inventory.RangedWeapon.AddAmmo(ammoAmount);
            Debug.Log("Подобрано патронов: " + ammoAmount);
            Destroy(gameObject);
        }
    }
}