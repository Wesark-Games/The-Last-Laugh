using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.UI
{
    public class WeaponHUD : MonoBehaviour
    {
        [SerializeField] private Project.Player.WeaponInventory inventory;

        [Header("[ UI ЭЛЕМЕНТЫ ]")]
        [SerializeField] private Image weaponIcon;
        [SerializeField] private TextMeshProUGUI weaponName;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private TextMeshProUGUI reloadText;
        [SerializeField] private Image reloadBar;

        private Project.Combat.Weapon _trackedWeapon;
        private float _reloadStartTime;
        private float _reloadDuration;
        private bool _isReloading;

        private void Start()
        {
            if (inventory == null) return;
            inventory.OnWeaponChanged += OnWeaponChanged;
            OnWeaponChanged(inventory.CurrentIndex, inventory.CurrentSlot);
        }

        private void OnDestroy()
        {
            if (inventory != null) inventory.OnWeaponChanged -= OnWeaponChanged;
            UnsubscribeFromWeapon();
        }

        private void Update()
        {
            if (_isReloading && reloadBar != null)
            {
                float elapsed = Time.time - _reloadStartTime;
                reloadBar.fillAmount = Mathf.Clamp01(elapsed / _reloadDuration);
            }
        }

        private void OnWeaponChanged(int index, Project.Player.WeaponInventory.WeaponSlot slot)
        {
            UnsubscribeFromWeapon();
            if (slot == null) return;

            if (weaponIcon != null)
            {
                weaponIcon.sprite = slot.icon;
                weaponIcon.enabled = slot.icon != null;
            }
            if (weaponName != null) weaponName.text = slot.name;
            if (reloadText != null) reloadText.gameObject.SetActive(false);
            if (reloadBar != null) reloadBar.gameObject.SetActive(false);

            if (slot.isMelee || slot.weapon == null)
            {
                if (ammoText != null) ammoText.text = "∞";
                return;
            }

            _trackedWeapon = slot.weapon;
            _trackedWeapon.OnAmmoChanged += OnAmmoChanged;
            _trackedWeapon.OnReloadStart += OnReloadStart;
            _trackedWeapon.OnReloadEnd   += OnReloadEnd;

            OnAmmoChanged(_trackedWeapon.CurrentAmmo, _trackedWeapon.Data.MagazineSize);
        }

        private void UnsubscribeFromWeapon()
        {
            if (_trackedWeapon == null) return;
            _trackedWeapon.OnAmmoChanged -= OnAmmoChanged;
            _trackedWeapon.OnReloadStart -= OnReloadStart;
            _trackedWeapon.OnReloadEnd   -= OnReloadEnd;
            _trackedWeapon = null;
        }

        private void OnAmmoChanged(int current, int max)
        {
            if (ammoText != null) ammoText.text = $"{current} / {max}";
        }

        private void OnReloadStart()
        {
            _isReloading = true;
            _reloadStartTime = Time.time;
            _reloadDuration = _trackedWeapon != null ? _trackedWeapon.Data.ReloadTime : 1f;

            if (reloadText != null)
            {
                reloadText.text = "ПЕРЕЗАРЯДКА...";
                reloadText.gameObject.SetActive(true);
            }
            if (reloadBar != null)
            {
                reloadBar.gameObject.SetActive(true);
                reloadBar.fillAmount = 0f;
            }
        }

        private void OnReloadEnd()
        {
            _isReloading = false;
            if (reloadText != null) reloadText.gameObject.SetActive(false);
            if (reloadBar != null) reloadBar.gameObject.SetActive(false);
        }
    }
}