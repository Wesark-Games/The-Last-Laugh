using UnityEngine;

namespace Project.Combat
{
    public class HealthPickup : MonoBehaviour
    {
        [SerializeField] private int healAmount = 25;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            other.GetComponent<Health>()?.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
