using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.UI
{
    public class HealthHUD : MonoBehaviour
    {
        [SerializeField] private Project.Combat.Health playerHealth;
        [SerializeField] private Image healthFill;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private float fillSpeed = 5f;

        [Header("[ ЦВЕТА СОСТОЯНИЙ ]")]
        [SerializeField] private Color normalColor   = new Color(0.7f, 0.2f, 0.18f);
        [SerializeField] private Color lowColor      = new Color(0.9f, 0.3f, 0.1f);
        [SerializeField] private Color criticalColor = new Color(1f, 0.1f, 0.1f);

        private float _targetFill = 1f;

        private void Start()
        {
            if (playerHealth == null) return;
            playerHealth.OnHealthChanged += OnHealthChanged;
            OnHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        private void OnDestroy()
        {
            if (playerHealth != null) playerHealth.OnHealthChanged -= OnHealthChanged;
        }

        private void Update()
        {
            if (healthFill == null) return;
            healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, _targetFill, Time.deltaTime * fillSpeed);
        }

        private void OnHealthChanged(int current, int max)
        {
            _targetFill = (float)current / max;

            if (healthText != null) healthText.text = $"{current} / {max}";

            if (healthFill != null)
            {
                if (_targetFill <= 0.2f)      healthFill.color = criticalColor;
                else if (_targetFill <= 0.5f) healthFill.color = lowColor;
                else                          healthFill.color = normalColor;
            }
        }
    }
}