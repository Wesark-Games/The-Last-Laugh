using UnityEngine;

namespace Project.NPC
{
    /// <summary>
    /// Звуки шагов NPC — работает так же как в PlayerController.
    /// Добавляй на NPC вместе с NPCMovement.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class NPCFootsteps : MonoBehaviour
    {
        // ─── CONFIGURATION ────────────────────────────────────────────────
        [Header("[ ЗВУКИ ]")]
        [SerializeField] private AudioClip[] woodFootsteps;
        [SerializeField] private AudioClip[] tileFootsteps;
        [SerializeField] private float stepInterval = 0.4f;
        [Range(0f, 1f)]
        [SerializeField] private float stepVolume = 0.4f;

        [Header("[ СЛОИ ПОВЕРХНОСТЕЙ ]")]
        [SerializeField] private float surfaceCheckRadius = 0.15f;
        // ─────────────────────────────────────────────────────────────────

        private AudioSource audioSource;
        private Rigidbody2D rb;
        private float       stepTimer;
        private int         woodLayer;
        private int         tileLayer;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            rb          = GetComponent<Rigidbody2D>();

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
                if (audioSource.isPlaying)
                    audioSource.Stop();
                stepTimer = 0f;
            }
        }

        private void PlayStep()
        {
            AudioClip[] clips = GetClipsForSurface();
            if (clips == null || clips.Length == 0) return;

            AudioClip clip = clips[Random.Range(0, clips.Length)];
            audioSource.clip   = clip;
            audioSource.volume = stepVolume;
            audioSource.Play();
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