// PlayerDoorInteraction.cs — вешается на игрока
// Ничего особенного не нужно! Физика работает сама через Rigidbody2D.
// Этот скрипт нужен только если хочешь кнопку "открыть" вместо толчка.

using UnityEngine;

public class PlayerDoorInteraction : MonoBehaviour
{
    // Если хочешь открывать дверь кнопкой (E) а не толчком:
    [Header("Открытие кнопкой (опционально)")]
    public float pushForce = 300f;
    public KeyCode interactKey = KeyCode.E;

    private Door doorInRange;

    void Update()
    {
        if (doorInRange != null && Input.GetKeyDown(interactKey))
        {
            Rigidbody2D doorRb = doorInRange.GetComponent<Rigidbody2D>();
            if (doorRb != null)
            {
                // Определяем направление толчка относительно петли
                Vector2 dirToDoor = (doorInRange.transform.position 
                                   - transform.position).normalized;
                float torqueDir = Vector2.SignedAngle(Vector2.right, dirToDoor) > 0 
                                  ? 1f : -1f;
                doorRb.AddTorque(pushForce * torqueDir);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Door>(out Door door))
            doorInRange = door;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Door>(out Door door) && door == doorInRange)
            doorInRange = null;
    }
}