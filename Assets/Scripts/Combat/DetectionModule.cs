using UnityEngine;

namespace Project.Combat
{
    public class DetectionModule : EnemyModule
    {
        [SerializeField] private float detectionRadius = 5f;
        [SerializeField] private float losePlayerTime  = 3f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask wallLayer;

        private float _timeSinceLastSeen;

        public override void UpdateModule()
        {
            var hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

            if (hit != null && HasLineOfSight(hit.transform))
            {
                Core.SetTarget(hit.transform);
                Core.SetState(EnemyState.Chasing);
                _timeSinceLastSeen = 0f;
            }
            else if (Core.Target != null)
            {
                _timeSinceLastSeen += Time.deltaTime;
                if (_timeSinceLastSeen >= losePlayerTime)
                {
                    Core.SetTarget(null);
                    Core.SetState(EnemyState.Idle);
                    _timeSinceLastSeen = 0f;
                }
            }
        }

        public void ForceAlert(Transform target)
        {
            Core.SetTarget(target);
            Core.SetState(EnemyState.Chasing);
            _timeSinceLastSeen = 0f;
        }

        private bool HasLineOfSight(Transform target)
        {
            if (wallLayer == 0) return true;
            Vector2 dir  = target.position - transform.position;
            float   dist = dir.magnitude;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, dist, wallLayer);
            return hit.collider == null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
