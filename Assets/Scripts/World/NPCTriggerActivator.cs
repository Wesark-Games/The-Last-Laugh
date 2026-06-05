using UnityEngine;
using UnityEngine.Events;

namespace Project.World
{
    /// <summary>
    /// Запускает NPC после определённого события.
    /// Вешай на NPC. Вызывай Activate() из диалога, триггера или другого скрипта.
    /// </summary>
    public class NPCTriggerActivator : MonoBehaviour
    {
        // ─── CONFIGURATION ────────────────────────────────────────────────
        [Header("[ АКТИВАЦИЯ ]")]
        [Tooltip("Тип движения который запустится после активации")]
        [SerializeField] private string movementTypeOnActivate = "Waypoints";
        [Tooltip("Задержка перед стартом движения")]
        [SerializeField] private float  activationDelay        = 0f;

        [Header("[ СОСТОЯНИЕ ДО АКТИВАЦИИ ]")]
        [Tooltip("Фраза которую NPC говорит до активации (через DialogueSystem)")]
        [SerializeField] private string idleDialogueLine = "";
        [Tooltip("Направление взгляда до активации")]
        [SerializeField] private Vector2 idleFacingDirection = Vector2.down;

        [Header("[ СОБЫТИЯ ]")]
        public UnityEvent onActivated;
        // ─────────────────────────────────────────────────────────────────

      private Project.Movement.NPCMovement  npcMovement;
        private Project.NPC.DialogueSystem    dialogueSystem;
        private bool                          isActivated = false;

        public bool IsActivated => isActivated;

        private void Awake()
        {
            npcMovement    = GetComponent<Project.Movement.NPCMovement>();
            dialogueSystem = GetComponent<Project.NPC.DialogueSystem>();
        }

        private void Start()
        {
            // Устанавливаем начальное состояние
            if (npcMovement != null)
                npcMovement.enabled = false;

            // Направление взгляда
            npcMovement?.SetFacingDirection(idleFacingDirection);
        }

        /// <summary>
        /// Активировать NPC. Вызывай из onLastLineReached в NPCInteraction
        /// или из любого другого события.
        /// </summary>
        public void Activate()
        {
            if (isActivated) return;
            isActivated = true;

            if (dialogueSystem != null)
                dialogueSystem.Hide();

            if (activationDelay > 0f)
                StartCoroutine(ActivateWithDelay());
            else
                DoActivate();
        }

        private System.Collections.IEnumerator ActivateWithDelay()
        {
            yield return new WaitForSeconds(activationDelay);
            DoActivate();
        }

public void ShowIdleLine()
        {
            // Показываем фразу только если NPC еще не активирован
            if (!isActivated && !string.IsNullOrEmpty(idleDialogueLine) && dialogueSystem != null)
            {
                dialogueSystem.ShowLine(idleDialogueLine);
            }
        }

        private void DoActivate()
        {
            if (npcMovement != null)
            {
                npcMovement.enabled = true;
                npcMovement.StartMoving(movementTypeOnActivate);
            }

            onActivated?.Invoke();
        }
    }
}