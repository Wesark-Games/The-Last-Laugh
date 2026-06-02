using UnityEngine;

namespace Project.Combat
{
    public class ItemDrop : MonoBehaviour
    {
        [SerializeField] private GameObject healthPickupPrefab;
        [SerializeField] [Range(0f, 1f)] private float dropChance = 0.5f;

        private void Awake()
        {
            var health = GetComponent<Health>();
            if (health != null)
                health.OnDeath += OnDeath;
        }

        private void OnDeath()
        {
            if (healthPickupPrefab == null) return;
            if (Random.value <= dropChance)
                Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
        }
    }
}
