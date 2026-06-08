using UnityEngine;
using UnityEngine.Events;

namespace Project.SaveSystem
{
  
    public class CheckpointManager : MonoBehaviour
    {
      
        [Header("[ ЧЕКПОИНТ ]")]
        [SerializeField] private int    checkpointIndex = 0;
        [SerializeField] private bool   triggerOnce     = true;
        [SerializeField] private float  saveDelay       = 0.5f;

        [Header("[ ЗАДАНИЕ ПРИ ЧЕКПОИНТЕ ]")]

        [SerializeField] private string questTextOnCheckpoint = "";

        [Header("[ СОБЫТИЯ ]")]
        public UnityEvent onCheckpointReached;

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