using UnityEngine;

namespace Project.Combat
{
    public class ItemDrop : MonoBehaviour
    {
        [Header("[ ОРУЖИЕ ]")]
        [SerializeField] private GameObject weaponPickupPrefab;
        [SerializeField] [Range(0f, 1f)] private float weaponDropChance = 0.3f;

        [Header("[ ПАТРОНЫ ]")]
        [SerializeField] private GameObject ammoPickupPrefab;
        [SerializeField] [Range(0f, 1f)] private float ammoDropChance = 0.4f;

        [Header("[ АПТЕЧКА ]")]
        [SerializeField] private GameObject healthPickupPrefab;
        [SerializeField] [Range(0f, 1f)] private float healthDropChance = 0.4f;

        private void Awake()
        {
            var health = GetComponent<Health>();
            if (health != null)
            {
                health.OnDeath += OnDeath;
                Debug.Log("ItemDrop подписался на смерть врага");
            }
            else
            {
                Debug.Log("ItemDrop НЕ НАШЁЛ Health на этом объекте!");
            }
        }

        private void OnDeath()
        {
            Debug.Log("Враг умер! weaponPrefab: " + (weaponPickupPrefab != null)
                      + " | ammoPrefab: " + (ammoPickupPrefab != null)
                      + " | healthPrefab: " + (healthPickupPrefab != null));

            if (weaponPickupPrefab != null && Random.value <= weaponDropChance)
            {
                Debug.Log("Выпало ОРУЖИЕ");
                Instantiate(weaponPickupPrefab, transform.position, Quaternion.identity);
                return;
            }
            if (ammoPickupPrefab != null && Random.value <= ammoDropChance)
            {
                Debug.Log("Выпали ПАТРОНЫ");
                Instantiate(ammoPickupPrefab, transform.position, Quaternion.identity);
                return;
            }
            if (healthPickupPrefab != null && Random.value <= healthDropChance)
            {
                Debug.Log("Выпала АПТЕЧКА");
                Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}