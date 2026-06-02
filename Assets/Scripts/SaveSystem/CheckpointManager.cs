using UnityEngine;
using UnityEngine.Events;

namespace Project.SaveSystem
{
    /// <summary>
    /// Чекпоинт — триггер автосохранения.
    /// Вешай на невидимые зоны или объекты в сцене.
    /// </summary>
    public class CheckpointManager : MonoBehaviour
    {
        // ─── CONFIGURATION ────────────────────────────────────────────────
        [Header("[ ЧЕКПОИНТ ]")]
        [SerializeField] private int    checkpointIndex = 0;
        [SerializeField] private bool   triggerOnce     = true;
        [Tooltip("Задержка перед сохранением (чтобы не мешало геймплею)")]
        [SerializeField] private float  saveDelay       = 0.5f;

        [Header("[ ЗАДАНИЕ ПРИ ЧЕКПОИНТЕ ]")]
        [Tooltip("Текст задания который активируется в этом чекпоинте")]
        [SerializeField] private string questTextOnCheckpoint = "";

        [Header("[ СОБЫТИЯ ]")]
        public UnityEvent onCheckpointReached;
        // ─────────────────────────────────────────────────────────────────

        private bool triggered = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (triggerOnce && triggered)   return;

            triggered = true;
            StartCoroutine(SaveWithDelay());
            onCheckpointReached?.Invoke();
        }

        private System.Collections.IEnumerator SaveWithDelay()
        {
            yield return new WaitForSeconds(saveDelay);

            if (SaveManager.Instance == null) yield break;

            // Сохраняем задание если указано
            if (!string.IsNullOrEmpty(questTextOnCheckpoint))
                SaveManager.Instance.SetActiveQuest(questTextOnCheckpoint, checkpointIndex);

            SaveManager.Instance.SaveAtCheckpoint(checkpointIndex);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
            Gizmos.DrawCube(transform.position, Vector3.one);
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.8f);
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}