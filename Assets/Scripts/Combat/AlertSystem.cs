using UnityEngine;

namespace Project.Combat
{
    public class AlertSystem : MonoBehaviour
    {
        public static AlertSystem Instance { get; private set; }

        [SerializeField] private float alertRadius = 8f;
        [SerializeField] private LayerMask enemyLayer;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void AlertNearbyEnemies(Vector2 position, Transform target)
        {
            var hits = Physics2D.OverlapCircleAll(position, alertRadius, enemyLayer);
            foreach (var hit in hits)
            {
                hit.GetComponent<DetectionModule>()?.ForceAlert(target);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, alertRadius);
        }
    }
}
