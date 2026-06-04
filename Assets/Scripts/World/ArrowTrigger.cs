using UnityEngine;

namespace Project.World
{
    public class ArrowTrigger : MonoBehaviour
    {
        [Tooltip("Перетащи сюда главный объект Arrows, на котором висит ArrowHider")]
        [SerializeField] private ArrowHider mainHider;

        private bool triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered) return;
            if (!other.CompareTag("Player")) return;

            if (mainHider != null)
            {
                triggered = true;
                mainHider.AdvanceSequence(); // Прямой приказ менеджеру переключить шаг
                
                // Если скрипт висит прямо на стрелочке, выключать объект кнопкой SetActive нельзя, 
                // иначе корутина затухания в ArrowHider сломается. 
                // Поэтому просто отключаем этот триггер, чтобы он не срабатывал дважды.
                if (TryGetComponent<Collider2D>(out var col))
                {
                    col.enabled = false;
                }
            }
        }
    }
}