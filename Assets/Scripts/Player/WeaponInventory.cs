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
            // Клавиши 1..5 переключают слоты
            Key[] keys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5 };
            for (int i = 0; i < slots.Length && i < keys.Length; i++)
            {
                if (Keyboard.current[keys[i]].wasPressedThisFrame)
                    EquipSlot(i);
            }

            // Колесо мыши — переключение
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll > 0) EquipSlot((_currentIndex + 1) % slots.Length);
            else if (scroll < 0) EquipSlot((_currentIndex - 1 + slots.Length) % slots.Length);
        }

        public void EquipSlot(int index)
        {
            if (index < 0 || index >= slots.Length) return;
            if (slots[index] == null) return;
            if (slots[index].weapon == null && !slots[index].isMelee) return;

            // Отключаем все оружия
            foreach (var s in slots)
                if (s?.weapon != null) s.weapon.gameObject.SetActive(false);

            _currentIndex = index;
            if (slots[_currentIndex].weapon != null)
                slots[_currentIndex].weapon.gameObject.SetActive(true);

            OnWeaponChanged?.Invoke(_currentIndex, slots[_currentIndex]);
        }
    }
}
