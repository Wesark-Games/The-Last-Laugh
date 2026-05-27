using UnityEngine;
using TMPro;

namespace Project.UI
{
    public class ScoreHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;

        private void Start()
        {
            if (Project.Combat.ScoreManager.Instance != null)
            {
                Project.Combat.ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
                OnScoreChanged(Project.Combat.ScoreManager.Instance.Score);
            }
        }

        private void OnDestroy()
        {
            if (Project.Combat.ScoreManager.Instance != null)
                Project.Combat.ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
        }

        private void OnScoreChanged(int score)
        {
            if (scoreText != null) scoreText.text = $"Очки: {score}";
        }
    }
}
