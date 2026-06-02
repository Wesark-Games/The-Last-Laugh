using UnityEngine;

namespace Project.Combat
{
    public class ShootingModule : EnemyModule
    {
        [SerializeField] private WeaponData weaponData;
        [SerializeField] private float shootRange = 6f;
        [SerializeField] private float fireRate   = 1.5f;

        private float _nextFireTime;

        public override void UpdateModule()
        {
            if (Core.Target == null) return;
            if (weaponData == null || weaponData.ProjectilePrefab == null) return;

            float dist = Vector2.Distance(transform.position, Core.Target.position);

            // Дальний враг останавливается и стреляет
            if (Core.CurrentState == EnemyState.Chasing && dist <= shootRange)
            {
                Core.SetState(EnemyState.Attacking);
            }

            if (Core.CurrentState != EnemyState.Attacking) return;
            if (Time.time < _nextFireTime) return;

            _nextFireTime = Time.time + fireRate;

            Vector2 dir      = (Core.Target.position - transform.position).normalized;
            Vector3 spawnPos = transform.position + (Vector3)(dir * 0.7f);

            GameObject bulletObj = Instantiate(weaponData.ProjectilePrefab, spawnPos, Quaternion.identity);
            bulletObj.GetComponent<Projectile>()?.Init(dir, weaponData.Damage, weaponData.BulletSpeed, false);
        }
    }
}
