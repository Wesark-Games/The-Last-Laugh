using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Player
{
    public class WeaponInventory : MonoBehaviour
    {
        [Serializable]
        public class WeaponSlot
        {
            public string name;
            public Project.Combat.Weapon weapon;
            public Sprite icon;
            public bool isMelee;
            public bool unlocked = true;
        }

        [SerializeField] private WeaponSlot[] slots = new WeaponSlot[5];
        private int _currentIndex = 0;

        public event Action<int, WeaponSlot> OnWeaponChanged;

        public WeaponSlot CurrentSlot => (_currentIndex >= 0 && _currentIndex < slots.Length) ? slots[_currentIndex] : null;
        public Project.Combat.Weapon CurrentWeapon => CurrentSlot?.weapon;
        public int CurrentIndex => _currentIndex;
        public WeaponSlot[] Slots => slots;

        private void Start()
        {
            EquipSlot(0);
        }

        private void Update()
        {
            Key[] keys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5 };
            for (int i = 0; i < slots.Length && i < keys.Length; i++)
            {
                if (Keyboard.current[keys[i]].wasPressedThisFrame)
                    EquipSlot(i);
            }

            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll > 0) EquipNextUnlocked(1);
            else if (scroll < 0) EquipNextUnlocked(-1);
        }

        public void EquipSlot(int index)
        {
            if (index < 0 || index >= slots.Length) return;
            if (slots[index] == null) return;
            if (!slots[index].unlocked) return;
            if (slots[index].weapon == null && !slots[index].isMelee) return;

            foreach (var s in slots)
                if (s?.weapon != null) s.weapon.gameObject.SetActive(false);

            _currentIndex = index;
            if (slots[_currentIndex].weapon != null)
                slots[_currentIndex].weapon.gameObject.SetActive(true);

            OnWeaponChanged?.Invoke(_currentIndex, slots[_currentIndex]);
        }

        private void EquipNextUnlocked(int dir)
        {
            int idx = _currentIndex;
            for (int i = 0; i < slots.Length; i++)
            {
                idx = (idx + dir + slots.Length) % slots.Length;
                if (slots[idx] != null && slots[idx].unlocked &&
                    (slots[idx].weapon != null || slots[idx].isMelee))
                {
                    EquipSlot(idx);
                    return;
                }
            }
        }

        // Подбор оружия с земли
        public bool PickupWeapon(int slotIndex, Project.Combat.WeaponData data)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length) return false;
            if (slots[slotIndex] == null) return false;
            if (slots[slotIndex].weapon == null) return false;

            // Меняем данные оружия в слоте и разблокируем
            slots[slotIndex].weapon.SetWeaponData(data);
            slots[slotIndex].unlocked = true;
            slots[slotIndex].name = data.name;

            // Сразу экипируем подобранное
            EquipSlot(slotIndex);
            return true;
        }
    }
}
