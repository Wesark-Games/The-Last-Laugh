using UnityEngine;

namespace Project.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class NPCMovement : MonoBehaviour
    {
        public enum MovementType { Idle, Waypoints, RandomRoaming }

        [Header("[ НАСТРОЙКИ ДВИЖЕНИЯ ]")]
        [SerializeField] private MovementType movementType = MovementType.Idle;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float minWaitTime = 1f;
        [SerializeField] private float maxWaitTime = 3f;

        [Header("[ ПАТРУЛИРОВАНИЕ ]")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private bool stopAtLastWaypoint = true;
        
        [Header("[ СЛУЧАЙНЫЕ ТРЕКИ ]")]
        [SerializeField] private float roamRadius = 4f;

        // Компоненты
        private Rigidbody2D rb;
        private Animator anim;
        private SpriteRenderer spriteRenderer;

        // Внутренние переменные логики
        private Vector2 targetPosition;
        private Vector2 moveDirection;
        private float waitTimer;
        private int currentWaypointIndex;
        private bool isWaiting;
        private Vector2 startPosition;
        private Vector2 facingDirection;
        
        private bool canMove = false;
        private bool routeCompleted = false; // Флаг завершения маршрута

        // Кэшируем хэши параметров
        private static readonly int DirXHash = Animator.StringToHash("DirX");
        private static readonly int DirYHash = Animator.StringToHash("DirY");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            
            rb.gravityScale = 0f;
            rb.freezeRotation = true; 
            
            facingDirection = Vector2.down;
        }

        private void Start()
        {
            startPosition = transform.position;
            
            if (movementType != MovementType.Idle)
            {
                canMove = true;
                GetNextTarget();
            }
        }

        private void Update()
        {
            if (!canMove || movementType == MovementType.Idle)
            {
                moveDirection = Vector2.zero;
                UpdateAnimator(Vector2.zero);
                return;
            }

            if (isWaiting)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    GetNextTarget();
                }
            }
            else
            {
                Vector2 currentPos = transform.position;
                moveDirection = (targetPosition - currentPos).normalized;

                if (Vector2.Distance(currentPos, targetPosition) < 0.1f)
                {
                    StartWaiting();
                }
            }

            UpdateAnimator(moveDirection);
        }

        private void FixedUpdate()
        {
            if (canMove && movementType != MovementType.Idle && !isWaiting && !routeCompleted)
            {
                rb.linearVelocity = moveDirection * moveSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        public void StartMoving(string typeName)
        {
            if (System.Enum.TryParse(typeName, out MovementType newType))
            {
                movementType = newType;
                if (movementType != MovementType.Idle)
                {
                    canMove = true;
                    isWaiting = false;
                    routeCompleted = false;
                    currentWaypointIndex = 0; // Сбрасываем индекс на начало при новом старте
                    GetNextTarget();
                }
            }
        }

        private void StartWaiting()
        {
            if (routeCompleted)
            {
                moveDirection = Vector2.zero;
                return;
            }

            isWaiting = true;
            moveDirection = Vector2.zero;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
        }

        private void GetNextTarget()
        {
            if (movementType == MovementType.Waypoints)
            {
                if (waypoints == null || waypoints.Length == 0) return;

                if (currentWaypointIndex >= waypoints.Length)
                {
                    if (stopAtLastWaypoint)
                    {
                        routeCompleted = true;
                        movementType = MovementType.Idle;
                        moveDirection = Vector2.zero;
                        return;
                    }
                    else
                    {
                        currentWaypointIndex = 0;
                    }
                }

                targetPosition = waypoints[currentWaypointIndex].position;
                
                if (stopAtLastWaypoint && currentWaypointIndex == waypoints.Length - 1)
                {
                    routeCompleted = true;
                }

                currentWaypointIndex++;
            }
            else if (movementType == MovementType.RandomRoaming)
            {
                Vector2 randomCircle = Random.insideUnitCircle * roamRadius;
                targetPosition = startPosition + randomCircle;
            }
        }

        private void UpdateAnimator(Vector2 velocity)
        {
            if (anim == null) return;

            // Если маршрут завершен или тип сменился на Idle, принудительно гасим движение в аниматоре
            bool isMoving = velocity.sqrMagnitude > 0.05f && !routeCompleted && movementType != MovementType.Idle;

            if (isMoving)
                facingDirection = GetDirection8Way(velocity);

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

            anim.SetFloat(DirXHash, animX);
            anim.SetFloat(DirYHash, animY);
            anim.SetBool(IsMovingHash, isMoving);
        }

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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 startPos = Application.isPlaying ? (Vector3)startPosition : transform.position;
            Gizmos.DrawWireSphere(startPos, roamRadius);
        }

    
        public void SetFacingDirection(Vector2 direction)
        {
            facingDirection = GetDirection8Way(direction);
            ApplyAnimation(facingDirection, false);
        }

        // Методы для системы сохранений

        public string GetCurrentMovementType()
        {
            return movementType.ToString();
        }

       
        public bool IsRouteCompleted()
        {
            return routeCompleted;
        }

    
        public void LoadState(string typeName, bool isRouteDone)
        {
            if (System.Enum.TryParse(typeName, out MovementType savedType))
            {
                movementType = savedType;
                routeCompleted = isRouteDone;

                if (movementType == MovementType.Idle || routeCompleted)
                {
                    canMove = false;
                    moveDirection = Vector2.zero;
                    if (rb != null) rb.linearVelocity = Vector2.zero;
                    UpdateAnimator(Vector2.zero);
                }
                else
                {
                    canMove = true;
                    isWaiting = false;
                    GetNextTarget();
                }
            }
        }
    }
}