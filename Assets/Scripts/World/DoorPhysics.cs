// Door.cs — вешается на DoorPivot
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Физика двери")]
    [Tooltip("Сила пружины возврата (0 = нет возврата)")]
    public float returnForce = 0f;

    [Tooltip("Угол закрытого положения (обычно 0)")]
    public float closedAngle = 0f;

    [Header("Звук (опционально)")]
    public AudioClip doorHitSound;

    private Rigidbody2D rb;
    private AudioSource audioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (returnForce > 0f)
        {
            float angleDiff = Mathf.DeltaAngle(rb.rotation, closedAngle);
            rb.AddTorque(angleDiff * returnForce * Time.fixedDeltaTime);
        }
    }

  
    void OnCollisionEnter2D(Collision2D col)
    {
    
        if (col.gameObject.CompareTag("DoorStop"))
        {
            float impactSpeed = Mathf.Abs(rb.angularVelocity);
            if (impactSpeed > 30f && doorHitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(doorHitSound, impactSpeed / 200f);
            }
        }
    }
}