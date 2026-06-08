using UnityEngine;

namespace Project.Combat
{
    public class ShootingModule : EnemyModule
    {
        [Header("[ СТРЕЛЬБА ]")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float fireRate = 1.2f;
        [SerializeField] private float shootRange = 7f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float bulletSpeed = 8f;
        [SerializeField] private float spread = 4f;

        private float _nextFireTime;

        public override void UpdateModule()
        {
            if (Core == null || Core.Target == null) return;

            Vector2 toTarget = (Vector2)Core.Target.position - (Vector2)transform.position;
            float dist = toTarget.magnitude;
            if (dist > shootRange) return;

            if (Time.time < _nextFireTime) return;
            _nextFireTime = Time.time + fireRate;

            Shoot(toTarget.normalized);
        }

        private void Shoot(Vector2 dir)
        {
            if (projectilePrefab == null) return;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            angle += Random.Range(-spread, spread);
            Vector2 shootDir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad));

            Vector3 spawnPos = transform.position + (Vector3)(shootDir * 0.7f);
            GameObject b = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            Projectile p = b.GetComponent<Projectile>();
            p?.Init(shootDir, damage, bulletSpeed, false); // false = пуля врага
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, shootRange);
        }
    }
}