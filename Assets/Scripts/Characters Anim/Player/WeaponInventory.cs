using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Player
{
    public class WeaponInventory : MonoBehaviour
    {
        [Header("[ РУКОПАШНЫЙ СЛОТ ]")]
        [SerializeField] private string meleeName = "Кулаки";
        [SerializeField] private Sprite meleeIcon;

        [Header("[ СЛОТ ОРУЖИЯ ]")]
        [SerializeField] private Project.Combat.Weapon rangedWeapon;
        [SerializeField] private SpriteRenderer rangedVisual;

        private bool _hasRangedWeapon;
        private bool _usingRanged;
        private string _rangedName;
        private Sprite _rangedIcon;

        public event Action OnWeaponChanged;

        public bool UsingRanged => _usingRanged && _hasRangedWeapon;
        public bool HasRangedWeapon => _hasRangedWeapon;
        public Project.Combat.Weapon RangedWeapon => rangedWeapon;
        public string CurrentName => _usingRanged && _hasRangedWeapon ? _rangedName : meleeName;
        public Sprite CurrentIcon => _usingRanged && _hasRangedWeapon ? _rangedIcon : meleeIcon;

        private void Start()
        {
            // Если оружие уже назначено — берём его спрайт и иконку
            if (rangedWeapon != null && rangedWeapon.Data != null)
            {
                _hasRangedWeapon = true;
                _rangedName = rangedWeapon.Data.name;
                _rangedIcon = rangedWeapon.Data.Icon;
                if (rangedVisual != null && rangedWeapon.Data.WeaponSprite != null)
                    rangedVisual.sprite = rangedWeapon.Data.WeaponSprite;
            }

            _usingRanged = false;
            UpdateVisual();
            OnWeaponChanged?.Invoke();
        }

        private void Update()
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                SwitchToMelee();

            if (Keyboard.current.digit2Key.wasPressedThisFrame && _hasRangedWeapon)
                SwitchToRanged();

            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                if (_usingRanged) SwitchToMelee();
                else if (_hasRangedWeapon) SwitchToRanged();
            }
        }

        private void SwitchToMelee()
        {
            _usingRanged = false;
            UpdateVisual();
            OnWeaponChanged?.Invoke();
        }

        private void SwitchToRanged()
        {
            if (!_hasRangedWeapon) return;
            _usingRanged = true;
            UpdateVisual();
            OnWeaponChanged?.Invoke();
        }

        // Подбор оружия — заменяет текущее дальнобойное и его спрайт
        public bool PickupWeapon(Project.Combat.WeaponData data, Sprite icon)
        {
            if (rangedWeapon == null) return false;

            rangedWeapon.SetWeaponData(data);
            _hasRangedWeapon = true;
            _rangedName = data.name;
            _rangedIcon = icon != null ? icon : data.Icon;

            // Меняем спрайт оружия в руках
            if (rangedVisual != null && data.WeaponSprite != null)
                rangedVisual.sprite = data.WeaponSprite;

            _usingRanged = true;
            UpdateVisual();
            OnWeaponChanged?.Invoke();
            return true;
        }

        private void UpdateVisual()
        {
            if (rangedVisual != null)
                rangedVisual.enabled = _usingRanged && _hasRangedWeapon;

            if (rangedWeapon != null)
                rangedWeapon.gameObject.SetActive(_usingRanged && _hasRangedWeapon);
        }
    }
}