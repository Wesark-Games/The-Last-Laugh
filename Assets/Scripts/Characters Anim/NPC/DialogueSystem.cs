using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.NPC
{
    /// <summary>
    /// Облачко с фразой над головой NPC.
    /// Требует: дочерний объект BubbleRoot с Image и TextMeshPro.
    /// </summary>
    public class DialogueSystem : MonoBehaviour
    {
        // ─── CONFIGURATION ────────────────────────────────────────────────
        [Header("[ ОБЛАЧКО ]")]
        [Tooltip("Корневой объект пузыря — содержит Image и Text")]
        [SerializeField] private GameObject bubbleRoot;
        [SerializeField] private TextMeshProUGUI bubbleText;
        [SerializeField] private Image           bubbleImage;

        [Header("[ АНИМАЦИЯ ]")]
        [SerializeField] private float fadeInDuration  = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [Tooltip("Задержка между появлением букв в секундах")]
        [SerializeField] private float typingSpeed = 0.04f;
        [Tooltip("Автоскрытие через N секунд ПОСЛЕ завершения печати. 0 = не скрывать автоматически")]
        [SerializeField] private float autoHideDuration = 0f;

        [Header("[ СМЕЩЕНИЕ НАД ГОЛОВОЙ ]")]
        [SerializeField] private Vector3 bubbleOffset = new Vector3(0.5f, 1.5f, 0f);

        [Header("[ ФРАЗЫ ПО УМОЛЧАНИЮ ]")]
        [Tooltip("Фразы которые NPC говорит при взаимодействии — листаются по порядку")]
        [SerializeField] private string[] dialogueLines;
        [Tooltip("true = зациклить фразы, false = остановиться на последней")]
        [SerializeField] private bool loopDialogue = false;
        // ─────────────────────────────────────────────────────────────────

        private CanvasGroup canvasGroup;
        private Coroutine   currentCoroutine;
        private Coroutine   typingCoroutine;
        private int         currentLineIndex = 0;
        private bool        isVisible        = false;

        public bool IsVisible => isVisible;
        public int  CurrentLineIndex => currentLineIndex;

        private void Awake()
        {
            if (bubbleRoot != null)
            {
                canvasGroup = bubbleRoot.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = bubbleRoot.AddComponent<CanvasGroup>();
            }

            Hide(instant: true);
        }

        private void LateUpdate()
        {
            // Всегда держим облачко над головой
            if (bubbleRoot != null && bubbleRoot.activeSelf)
                bubbleRoot.transform.position = transform.position + bubbleOffset;
        }

        // ─── ПУБЛИЧНЫЕ МЕТОДЫ ─────────────────────────────────────────────

        /// <summary>
        /// Показать следующую фразу из списка
        /// </summary>
        public void ShowNextLine()
        {
            if (dialogueLines == null || dialogueLines.Length == 0) return;

            ShowLine(dialogueLines[currentLineIndex]);

            currentLineIndex++;

            if (currentLineIndex >= dialogueLines.Length)
                currentLineIndex = loopDialogue ? 0 : dialogueLines.Length - 1;
        }

        /// <summary>
        /// Показать конкретную фразу с эффектом печатной машинки
        /// </summary>
        public void ShowLine(string text)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
                
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            currentCoroutine = StartCoroutine(ShowRoutine(text));
        }

        /// <summary>
        /// Скрыть облачко
        /// </summary>
        public void Hide(bool instant = false)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
                
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            if (instant)
            {
                isVisible = false;
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                if (bubbleRoot  != null) bubbleRoot.SetActive(false);
                return;
            }

            currentCoroutine = StartCoroutine(HideRoutine());
        }

        /// <summary>
        /// Сбросить индекс фраз на начало
        /// </summary>
        public void ResetDialogue() => currentLineIndex = 0;

        // ─── КОРУТИНЫ ────────────────────────────────────────────────────

        private IEnumerator ShowRoutine(string text)
        {
            isVisible = true;

            if (bubbleRoot != null) bubbleRoot.SetActive(true);

            // Плавное появление облачка
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeInDuration;
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            if (canvasGroup != null) canvasGroup.alpha = 1f;

            // Запускаем печать текста только после того, как облачко проявилось
            typingCoroutine = StartCoroutine(TypeTextRoutine(text));
        }

        private IEnumerator TypeTextRoutine(string fullText)
        {
            if (bubbleText == null) yield break;

            bubbleText.text = "";

            foreach (char letter in fullText.ToCharArray())
            {
                bubbleText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            // Автоскрытие срабатывает только ПОСЛЕ того, как весь текст напечатался
            if (autoHideDuration > 0f)
            {
                yield return new WaitForSeconds(autoHideDuration);
                currentCoroutine = StartCoroutine(HideRoutine());
            }
        }

        private IEnumerator HideRoutine()
        {
            float t = 0f;
            float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

            while (t < 1f)
            {
                t += Time.deltaTime / fadeOutDuration;
                if (canvasGroup != null)
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                yield return null;
            }

            isVisible = false;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (bubbleRoot  != null) bubbleRoot.SetActive(false);
        }
    }
}