using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Player
{
    public class PlayerShooter : MonoBehaviour
    {
        [SerializeField] private WeaponInventory inventory;
        [SerializeField] private MeleeCombat melee;
        [SerializeField] private Camera gameCamera;
        [SerializeField] private Transform characterTransform;

        private void Awake()
        {
            if (gameCamera == null) gameCamera = Camera.main;
            if (inventory == null) inventory = GetComponentInParent<WeaponInventory>();
            if (melee == null) melee = GetComponentInParent<MeleeCombat>();
            if (characterTransform == null)
            {
                var rb = GetComponentInChildren<Rigidbody2D>();
                if (rb != null) characterTransform = rb.transform;
            }
        }

        private void Update()
        {
            if (characterTransform == null || inventory == null)
            {
                Debug.Log("НЕТ ССЫЛКИ! characterTransform: " + (characterTransform != null) + " | inventory: " + (inventory != null));
                return;
            }

            Vector2 mousePos = gameCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 dir = (mousePos - (Vector2)characterTransform.position).normalized;

            if (Mouse.current.leftButton.isPressed)
            {
                Debug.Log("ЛКМ нажата. UsingRanged: " + inventory.UsingRanged);

                if (inventory.UsingRanged)
                {
                    Debug.Log("Стреляю. RangedWeapon есть: " + (inventory.RangedWeapon != null));
                    inventory.RangedWeapon?.TryShoot(dir, true);
                    Project.Combat.AlertSystem.Instance?.AlertNearbyEnemies(characterTransform.position, characterTransform);
                }
                else
                {
                    if (Mouse.current.leftButton.wasPressedThisFrame)
                        melee?.TryAttack(dir);
                }
            }

            if (Keyboard.current.rKey.wasPressedThisFrame && inventory.UsingRanged)
                inventory.RangedWeapon?.StartReload();
        }
    }
}