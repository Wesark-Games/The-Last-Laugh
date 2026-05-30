using UnityEngine;

namespace Project.NPC
{
    [RequireComponent(typeof(AudioSource))]
    public class NPCFootsteps : MonoBehaviour
    {
        [Header("[ ЗВУКИ ]")]
        [SerializeField] private AudioClip[] woodFootsteps;
        [SerializeField] private AudioClip[] tileFootsteps;
        [SerializeField] private float stepInterval = 0.4f;
        [Range(0f, 1f)]
        [SerializeField] private float stepVolume = 0.35f;

        [Header("[ ПОВЕРХНОСТИ ]")]
        [SerializeField] private float surfaceCheckRadius = 0.15f;

        private AudioSource audioSource;
        private Rigidbody2D rb;
        private float       stepTimer;
        private int         woodLayer;
        private int         tileLayer;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            rb          = GetComponent<Rigidbody2D>();

            // Громкость через AudioMixer — здесь не трогаем
            audioSource.playOnAwake = false;
            audioSource.loop        = false;

            woodLayer = LayerMask.NameToLayer("WoodFloor");
            tileLayer = LayerMask.NameToLayer("TileFloor");
        }

        private void Update()
        {
            if (rb == null) return;

            bool isMoving = rb.linearVelocity.sqrMagnitude > 0.1f;

            if (isMoving)
            {
                stepTimer -= Time.deltaTime;
                if (stepTimer <= 0f)
                {
                    PlayStep();
                    stepTimer = stepInterval;
                }
            }
            else
            {
                stepTimer = 0f;
                // PlayOneShot не нужно останавливать
            }
        }

        private void PlayStep()
        {
            AudioClip[] clips = GetClipsForSurface();
            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];

            // PlayOneShot — громкость через SFX группу миксера
            audioSource.PlayOneShot(clip, stepVolume);
        }

        private AudioClip[] GetClipsForSurface()
        {
            int mask = (1 << woodLayer) | (1 << tileLayer);
            Collider2D hit = Physics2D.OverlapCircle(transform.position, surfaceCheckRadius, mask);

            if (hit != null && hit.gameObject.layer == tileLayer)
                return tileFootsteps;

            return woodFootsteps;
        }
    }
}