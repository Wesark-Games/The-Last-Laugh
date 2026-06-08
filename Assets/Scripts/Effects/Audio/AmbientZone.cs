using UnityEngine;

public class AmbientZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioManager.Instance.ChangeZone(true);
        }
    }

   private void OnTriggerExit2D(Collider2D collision)
{
    if (!collision.CompareTag("Player")) return;

    // Проверяем, существует ли менеджер и не уничтожен ли он
    if (AudioManager.Instance != null && AudioManager.Instance.gameObject.activeInHierarchy)
    {
        // Передаем нужное состояние 
        AudioManager.Instance.ChangeZone(false); 
    }
}
    
}