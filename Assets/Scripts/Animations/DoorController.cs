using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator anim;
    private BoxCollider2D solidCollider;
    private Transform playerTransform;
    private SpriteRenderer playerRenderer;

    [Header("Настройки слоев")]
    [SerializeField] private int corridorOrder = 10;
    [SerializeField] private int roomOrder = 3;

    private bool isPlayerInsideTrigger = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
        foreach (var c in colliders)
        {
            if (!c.isTrigger)
            {
                solidCollider = c;
                break;
            }
        }
    }

    void Update()
    {
        // Если игрок находится в зоне двери, динамически управляем его слоем
        if (isPlayerInsideTrigger && playerTransform != null && playerRenderer != null)
        {
            // Сравниваем позицию ног игрока с позицией двери по оси Y
            if (playerTransform.position.y > transform.position.y)
            {
                playerRenderer.sortingOrder = roomOrder;
            }
            else
            {
                playerRenderer.sortingOrder = corridorOrder;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsideTrigger = true;
            playerTransform = other.transform;
            playerRenderer = other.GetComponent<SpriteRenderer>();
            
            OpenDoor();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsideTrigger = false;
            if (playerRenderer != null)
            {
                playerRenderer.sortingOrder = corridorOrder;
            }

            playerTransform = null;
            playerRenderer = null;
            
            CloseDoor();
        }
    }

    void OpenDoor()
    {
        if (anim != null)
        {
            anim.SetBool("isOpen", true);
        }
        
        if (solidCollider != null)
        {
            solidCollider.enabled = false; 
        }
    }

    void CloseDoor()
    {
        if (anim != null)
        {
            anim.SetBool("isOpen", false);
        }
        
        if (solidCollider != null)
        {
            solidCollider.enabled = true; 
        }
    }
}