using UnityEngine;
using UnityEngine.Events;
using Project.SaveSystem;
using System.Collections;

namespace Project.NPC
{
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

        [Header("[ СОХРАНЕНИЕ ДИАЛОГА ]")]
        [Tooltip("Уникальный ID этого NPC (например, npc_rabbit_prologue)")]
        [SerializeField] private string npcSaveID = "npc_bunny";

        private bool dialogueCompleted = false;

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

        private void Start()
        {
            // Ждем один кадр для безопасности, чтобы все синглтоны проснулись
            StartCoroutine(InitNPCDeferred());
        }

        private IEnumerator InitNPCDeferred()
        {
            yield return new WaitForEndOfFrame();

            // Проверяем статус завершения диалога
            if (SaveManager.Instance != null && SaveManager.Instance.GetFlag(npcSaveID + "_talked"))
            {
                dialogueCompleted = true;
                isInDialogue = false;
                playerInRange = false;
                
                // Железно включаем движение NPC обратно, чтобы он ходил по траектории
                if (npcMovement != null)
                    npcMovement.enabled = true;

                // Полностью прячем UI взаимодействия
                if (interactHint != null) 
                    interactHint.SetActive(false);
                
                if (dialogueSystem != null)
                {
                    dialogueSystem.ResetDialogue();
                    dialogueSystem.Hide(instant: true);
                    
                    // ДОПОЛНИТЕЛЬНАЯ ЗАЩИТА: Принудительно выключаем bubbleRoot, 
                    // чтобы облачко не висело фантомом над головой
                    var field = typeof(DialogueSystem).GetField("bubbleRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        GameObject bubble = field.GetValue(dialogueSystem) as GameObject;
                        if (bubble != null) bubble.SetActive(false);
                    }
                }
            }
        }

        private void Update()
        {
            if (dialogueCompleted) return;
            if (!playerInRange) return;

            if (interactHint != null && interactHint.activeSelf)
                interactHint.transform.position = transform.position + hintOffset;

            if (Input.GetKeyDown(interactKey))
                Interact();
        }

        private void LateUpdate()
        {
            if (dialogueCompleted) return;

            if (isInDialogue && facePlayerDuringDialogue && playerTransform != null)
                FacePlayer();
        }

        // ─── ТРИГГЕРЫ ────────────────────────────────────────────────────

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (dialogueCompleted) return;
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
            if (dialogueCompleted) return;

            if (!isInDialogue)
                StartDialogue();

            if (dialogueSystem == null) return;

            int totalLines = GetTotalLines();
            bool wasAtEnd = dialogueSystem.CurrentLineIndex >= totalLines - 1;

            if (wasAtEnd && isInDialogue)
            {
                onLastLineReached?.Invoke();
                EndDialogue();
                return;
            }

            dialogueSystem.ShowNextLine();
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

            // Возвращаем бег NPC на место
            if (npcMovement != null)
                npcMovement.enabled = true;

            if (dialogueSystem != null)
                dialogueSystem.Hide();

            // Если пролистали до конца — сохраняем статус
            if (dialogueSystem != null && dialogueSystem.CurrentLineIndex >= GetTotalLines() - 1)
            {
                dialogueCompleted = true;

                if (interactHint != null) interactHint.SetActive(false);

                if (SaveManager.Instance != null && !string.IsNullOrEmpty(npcSaveID))
                {
                    SaveManager.Instance.SetFlag(npcSaveID + "_talked");
                    
                    // Чтобы анимация сохранения в углу экрана не зависала из-за замороженного времени,
                    // возвращаем timeScale в 1 перед сохранением файлов
                    float oldTimeScale = Time.timeScale;
                    Time.timeScale = 1f;
                    
                    SaveManager.Instance.Save();
                    
                    Time.timeScale = oldTimeScale;
                }
            }

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