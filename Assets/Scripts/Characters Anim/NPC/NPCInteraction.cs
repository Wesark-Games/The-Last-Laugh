using UnityEngine;
using UnityEngine.Events;

namespace Project.NPC
{
    /// <summary>
    /// Триггер взаимодействия с NPC.
    /// Показывает подсказку "E" при приближении.
    /// По нажатию E — листает диалог.
    /// </summary>
    public class NPCInteraction : MonoBehaviour
    {
        // ─── CONFIGURATION ────────────────────────────────────────────────
        [Header("[ ВЗАИМОДЕЙСТВИЕ ]")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private DialogueSystem dialogueSystem;

        [Header("[ ПОДСКАЗКА ]")]
        [Tooltip("Объект с иконкой/текстом 'E' над NPC")]
        [SerializeField] private GameObject interactHint;
        [SerializeField] private Vector3    hintOffset = new Vector3(0f, 2.2f, 0f);

        [Header("[ ПОВЕДЕНИЕ ]")]
        [Tooltip("Остановить NPC во время диалога")]
        [SerializeField] private bool stopNPCDuringDialogue = true;
        [Tooltip("Повернуть NPC к игроку во время диалога")]
        [SerializeField] private bool facePlayerDuringDialogue = true;
        [Tooltip("Скрыть облачко когда игрок уходит")]
        [SerializeField] private bool hideOnExit = true;

        [Header("[ СОБЫТИЯ ]")]
        public UnityEvent onDialogueStart;
        public UnityEvent onDialogueEnd;
        public UnityEvent onLastLineReached;
        // ─────────────────────────────────────────────────────────────────

        private bool      playerInRange = false;
        private bool      isInDialogue  = false;
        private Transform playerTransform;
        private Project.Movement.NPCMovement npcMovement;

        private void Awake()
        {
            npcMovement = GetComponent<Project.Movement.NPCMovement>();

            if (interactHint != null)
                interactHint.SetActive(false);
        }

        private void Update()
        {
            if (!playerInRange) return;

            // Позиция подсказки
            if (interactHint != null && interactHint.activeSelf)
                interactHint.transform.position = transform.position + hintOffset;

            // Нажатие E
            if (Input.GetKeyDown(interactKey))
                Interact();
        }

        private void LateUpdate()
        {
            // Поворачиваем NPC к игроку во время диалога
            if (isInDialogue && facePlayerDuringDialogue && playerTransform != null)
                FacePlayer();
        }

        // ─── ТРИГГЕРЫ ────────────────────────────────────────────────────

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            playerInRange    = true;
            playerTransform  = other.transform;

            if (interactHint != null)
                interactHint.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            playerInRange = false;

            if (interactHint != null)
                interactHint.SetActive(false);

            if (isInDialogue)
                EndDialogue();

            if (hideOnExit && dialogueSystem != null)
                dialogueSystem.Hide();
        }

        // ─── ВЗАИМОДЕЙСТВИЕ ──────────────────────────────────────────────

        private void Interact()
        {
            if (!isInDialogue)
                StartDialogue();

            if (dialogueSystem == null) return;

            bool wasAtEnd = dialogueSystem.CurrentLineIndex >= GetTotalLines() - 1;

            dialogueSystem.ShowNextLine();

            if (wasAtEnd)
                onLastLineReached?.Invoke();
        }

        private void StartDialogue()
        {
            isInDialogue = true;

            if (stopNPCDuringDialogue && npcMovement != null)
                npcMovement.enabled = false;

            if (interactHint != null)
                interactHint.SetActive(false);

            onDialogueStart?.Invoke();
        }

        private void EndDialogue()
        {
            isInDialogue = false;

            if (stopNPCDuringDialogue && npcMovement != null)
                npcMovement.enabled = true;

            onDialogueEnd?.Invoke();
        }

        private void FacePlayer()
        {
            if (playerTransform == null) return;

            Vector2 dir = playerTransform.position - transform.position;

            Project.Movement.NPCMovement movement = GetComponent<Project.Movement.NPCMovement>();
            if (movement != null)
                movement.SetFacingDirection(dir.normalized);
        }

        private int GetTotalLines()
        {
            if (dialogueSystem == null) return 0;
            var field = typeof(DialogueSystem).GetField(
                "dialogueLines",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
            if (field == null) return 0;
            string[] lines = field.GetValue(dialogueSystem) as string[];
            return lines != null ? lines.Length : 0;
        }
    }
}