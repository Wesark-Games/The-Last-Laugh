using System;
using UnityEngine;

namespace Project.Combat
{
    public class Weapon : MonoBehaviour
    {
        [SerializeField] private WeaponData data;

        private float _nextFireTime;
        private int _currentAmmo;
        private bool _isReloading;

        public event Action<int, int> OnAmmoChanged;
        public event Action OnReloadStart;
        public event Action OnReloadEnd;

        public WeaponData Data => data;
        public bool IsReloading => _isReloading;
        public int CurrentAmmo => _currentAmmo;

        private void Awake()
        {
            if (data != null) _currentAmmo = data.MagazineSize;
        }

        public void SetWeaponData(WeaponData newData)
        {
            data = newData;
            _currentAmmo = data.MagazineSize;
            _isReloading = false;
            OnAmmoChanged?.Invoke(_currentAmmo, data.MagazineSize);
        }

        public bool TryShoot(Vector2 direction, bool isPlayerBullet = true)
        {
            if (data == null || _isReloading) return false;
            if (Time.time < _nextFireTime) return false;
            if (_currentAmmo <= 0) { StartReload(); return false; }

            _nextFireTime = Time.time + data.FireRate;
            _currentAmmo--;
            OnAmmoChanged?.Invoke(_currentAmmo, data.MagazineSize);

            SpawnBullet(direction, isPlayerBullet);

            if (_currentAmmo <= 0) StartReload();
            return true;
        }

        private void SpawnBullet(Vector2 direction, bool isPlayerBullet)
        {
            if (data.ProjectilePrefab == null) return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle += UnityEngine.Random.Range(-data.Spread, data.Spread);
            Vector2 spreadDir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad));

            Vector3 spawnPos = transform.position + (Vector3)(spreadDir * 0.6f);
            GameObject bulletObj = Instantiate(data.ProjectilePrefab, spawnPos, Quaternion.identity);
            Projectile projectile = bulletObj.GetComponent<Projectile>();
            projectile?.Init(spreadDir, data.Damage, data.BulletSpeed, isPlayerBullet);
        }

        public void StartReload()
        {
            if (_isReloading || data == null) return;
            if (_currentAmmo == data.MagazineSize) return;
            StartCoroutine(ReloadRoutine());
        }

        private System.Collections.IEnumerator ReloadRoutine()
        {
            _isReloading = true;
            OnReloadStart?.Invoke();
            yield return new WaitForSeconds(data.ReloadTime);
            _currentAmmo = data.MagazineSize;
            _isReloading = false;
            OnReloadEnd?.Invoke();
            OnAmmoChanged?.Invoke(_currentAmmo, data.MagazineSize);
        }
    }
}
