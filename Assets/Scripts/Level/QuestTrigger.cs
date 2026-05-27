using UnityEngine;

public class QuestTrigger : MonoBehaviour
{
    [Header("Quest Settings")]
    [Tooltip("Ссылка на QuestContainer, где висит скрипт QuestPanel")]
    [SerializeField] private QuestPanel questPanel;
    
    [TextArea(2, 4)]
    [SerializeField] private string questMessage = "Найти выход из цирка.";

    [Header("Trigger Options")]
    [Tooltip("Если включено, триггер сработает только один раз за игру")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем, что вошел игрок и триггер еще не отработал свой лимит
        if (collision.CompareTag("Player") && (!triggerOnlyOnce || !hasTriggered))
        {
            if (questPanel != null)
            {
                questPanel.ShowQuest(questMessage);
                hasTriggered = true;

                // Если нужен только один раз, можно полностью отключить коллайдер объекта
                if (triggerOnlyOnce)
                {
                    Collider2D col = GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;
                }
            }
            else
            {
                Debug.LogWarning($"На объекте {gameObject.name} не привязана ссылка на QuestPanel!");
            }
        }
    }
}