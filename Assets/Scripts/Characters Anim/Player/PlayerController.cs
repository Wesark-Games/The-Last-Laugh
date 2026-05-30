using UnityEngine;

namespace Project.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerController : MonoBehaviour
    {
        [Header("[ ДВИЖЕНИЕ ]")]
        [SerializeField] private float moveSpeed         = 4f;
        [SerializeField] private float movementSmoothing = 0.1f;

        [Header("[ ЗВУКИ ШАГОВ ]")]
        [SerializeField] private AudioClip[] woodFootsteps; // Массив шагов по дереву
        [SerializeField] private AudioClip[] tileFootsteps; // Массив шагов по плитке
        [SerializeField] private float stepInterval = 0.35f; 
        [SerializeField] private float stepVolume   = 0.6f;  

        [Header("[ ВВОД ]")]
        [SerializeField] private KeyCode upKey    = KeyCode.W;
        [SerializeField] private KeyCode downKey  = KeyCode.S;
        [SerializeField] private KeyCode leftKey  = KeyCode.A;
        [SerializeField] private KeyCode rightKey = KeyCode.D;

        private Rigidbody2D    rb;
        private Vector2        moveInput;
        private Vector2        currentVelocity;
        private Vector2        smoothVelocity;
        private Vector2        facingDirection;
        private Animator       animator;
        private SpriteRenderer spriteRenderer;
        private AudioSource    audioSource;

        private float          stepTimer;
        private int            woodLayerMask;
        private int            tileLayerMask;

        private static readonly int DirXHash     = Animator.StringToHash("DirX");
        private static readonly int DirYHash     = Animator.StringToHash("DirY");
        private static readonly int IsMovingHash  = Animator.StringToHash("IsMoving");

        // Свойство для перехвата управления внешними скриптами (автопилот)
        public Vector2 CustomInput { get; set; } = Vector2.zero;

        private void Awake()
        {
            rb             = GetComponent<Rigidbody2D>();
            animator       = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource    = GetComponent<AudioSource>();

            rb.gravityScale   = 0f;
            rb.freezeRotation = true;

            audioSource.playOnAwake = false;
            audioSource.loop        = false;

            facingDirection = Vector2.down;

            // Кэшируем индексы слоев для быстрой физической проверки
            woodLayerMask = LayerMask.NameToLayer("WoodFloor");
            tileLayerMask = LayerMask.NameToLayer("TileFloor");
        }

        private void OnEnable()
        {
            moveInput       = Vector2.zero;
            CustomInput     = Vector2.zero;
            currentVelocity = Vector2.zero;
            smoothVelocity  = Vector2.zero;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            if (animator != null)
                animator.SetBool(IsMovingHash, false);
        }

        private void Update()
        {
            ReadInput();
            HandleFootsteps();
        }

        private void FixedUpdate()
        {
            Move();
        }

        // ─── ЗВУКИ ШАГОВ И ОПРЕДЕЛЕНИЕ ПОВЕРХНОСТИ ────────────────────────

     // ─── ЗВУКИ ШАГОВ И ОПРЕДЕЛЕНИЕ ПОВЕРХНОСТИ ────────────────────────

        private void HandleFootsteps()
{
    if (currentVelocity.sqrMagnitude > 0.1f)
    {
        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            PlayFootstepSound();
            stepTimer = stepInterval;
        }
    }
    else
    {
        stepTimer = 0f;
        // PlayOneShot не нужно останавливать — звук доиграет сам
    }
}

       private void PlayFootstepSound()
{
    string      surface      = CheckCurrentSurface();
    AudioClip[] currentClips = surface == "Tile" ? tileFootsteps : woodFootsteps;

    if (currentClips == null || currentClips.Length == 0) return;

    AudioClip clip = currentClips[Random.Range(0, currentClips.Length)];

    // Громкость контролирует AudioMixer через группу SFX
    audioSource.PlayOneShot(clip, stepVolume);
}

        private string CheckCurrentSurface()
        {
            // Проверяем физическую точку под ногами игрока в радиусе 0.15 единиц
            // Используем маску, которая реагирует только на слои WoodFloor и TileFloor
            int combinedMask = (1 << woodLayerMask) | (1 << tileLayerMask);
            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.15f, combinedMask);

            if (hit != null)
            {
                int hitLayer = hit.gameObject.layer;
                
                if (hitLayer == woodLayerMask) return "Wood";
                if (hitLayer == tileLayerMask) return "Tile";
            }

            return "Wood"; // Возвращаем дерево, если игрок оказался вне настроенных коллайдеров пола
        }

        // Вызови этот метод в Unity редакторе (OnDrawGizmos), чтобы визуально видеть круг проверки под игроком
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.15f);
        }

        // ─── ВВОД ────────────────────────────────────────────────────────

        private void ReadInput()
        {
            if (CustomInput.sqrMagnitude > 0.01f)
            {
                moveInput = CustomInput;
                return;
            }

            float x = 0f;
            float y = 0f;

            if (Input.GetKey(rightKey)) x += 1f;
            if (Input.GetKey(leftKey))  x -= 1f;
            if (Input.GetKey(upKey))    y += 1f;
            if (Input.GetKey(downKey))  y -= 1f;

            moveInput = new Vector2(x, y);

            if (moveInput.sqrMagnitude > 1f)
                moveInput.Normalize();
        }

        // ─── НАПРАВЛЕНИЕ ─────────────────────────────────────────────────

        private Vector2 GetDirection8Way(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.01f) return facingDirection;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;

            if      (angle >= 22.5f  && angle < 67.5f)  return new Vector2( 1f,  1f);
            else if (angle >= 67.5f  && angle < 112.5f) return new Vector2( 0f,  1f);
            else if (angle >= 112.5f && angle < 157.5f) return new Vector2(-1f,  1f);
            else if (angle >= 157.5f && angle < 202.5f) return new Vector2(-1f,  0f);
            else if (angle >= 202.5f && angle < 247.5f) return new Vector2(-1f, -1f);
            else if (angle >= 247.5f && angle < 292.5f) return new Vector2( 0f, -1f);
            else if (angle >= 292.5f && angle < 337.5f) return new Vector2( 1f, -1f);
            else                                         return new Vector2( 1f,  0f);
        }

        // ─── ДВИЖЕНИЕ ────────────────────────────────────────────────────

        private void Move()
        {
            Vector2 targetVelocity = moveInput * moveSpeed;

            currentVelocity = Vector2.SmoothDamp(
                currentVelocity,
                targetVelocity,
                ref smoothVelocity,
                movementSmoothing
            );

            rb.linearVelocity = currentVelocity;
            UpdateAnimation(currentVelocity);
        }

        // ─── АНИМАЦИЯ ────────────────────────────────────────────────────

        private void UpdateAnimation(Vector2 velocity)
        {
            if (animator == null) return;

            bool isMoving = velocity.sqrMagnitude > 0.05f;
            bool hasInput = moveInput.sqrMagnitude > 0.01f;

            if (hasInput)
                facingDirection = GetDirection8Way(moveInput);

            ApplyAnimation(facingDirection, isMoving);
        }

        private void ApplyAnimation(Vector2 direction, bool isMoving)
        {
            float animX = direction.x;
            float animY = direction.y;

            if (direction.x > 0f)
            {
                if (spriteRenderer != null) spriteRenderer.flipX = true;
                animX = -direction.x;
            }
            else if (direction.x < 0f)
            {
                if (spriteRenderer != null) spriteRenderer.flipX = false;
            }
            else
            {
                if (spriteRenderer != null) spriteRenderer.flipX = false;
            }

            animator.SetFloat(DirXHash,    animX);
            animator.SetFloat(DirYHash,    animY);
            animator.SetBool(IsMovingHash, isMoving);
        }

        // ─── ПУБЛИЧНЫЕ МЕТОДЫ ─────────────────────────────────────────────

        public void SetMovementEnabled(bool enabled)
        {
            this.enabled = enabled;
            if (!enabled)
            {
                rb.linearVelocity = Vector2.zero;
                if (animator != null)
                    animator.SetBool(IsMovingHash, false);
            }
        }

        public void SetFacingDirection(Vector2 direction)
        {
            facingDirection = direction.normalized;
            ApplyAnimation(GetDirection8Way(facingDirection), false);
        }
    }
}