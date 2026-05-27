using UnityEngine;

namespace Project.Combat
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Combat/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [Header("[ УРОН ]")]
        public int Damage = 10;

        [Header("[ СТРЕЛЬБА ]")]
        public float FireRate = 0.3f;
        public float Spread = 3f;
        public float BulletSpeed = 12f;

        [Header("[ ПАТРОНЫ ]")]
        public int MagazineSize = 12;
        public float ReloadTime = 1.5f;

        [Header("[ СНАРЯД ]")]
        public GameObject ProjectilePrefab;
    }
}
