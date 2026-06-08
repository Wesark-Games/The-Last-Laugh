using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.NPC
{
    public class DialogueSystem : MonoBehaviour
    {
        [Header("[ ОБЛАЧКО ]")]
        [SerializeField] private GameObject bubbleRoot;
        [SerializeField] private TextMeshProUGUI bubbleText;
        [SerializeField] private Image           bubbleImage;

        [Header("[ АНИМАЦИЯ ]")]
        [SerializeField] private float fadeInDuration  = 0.2f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float typingSpeed = 0.04f;
        [SerializeField] private float autoHideDuration = 0f;

        [Header("[ СМЕЩЕНИЕ НАД ГОЛОВОЙ ]")]
        [SerializeField] private Vector3 bubbleOffset = new Vector3(0.5f, 1.5f, 0f);

        [Header("[ ФРАЗЫ ПО УМОЛЧАНИЮ ]")]
        [SerializeField] private string[] dialogueLines;
        [SerializeField] private bool loopDialogue = false;

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
            if (bubbleRoot != null && bubbleRoot.activeSelf)
                bubbleRoot.transform.position = transform.position + bubbleOffset;
        }


        public void ShowNextLine()
        {
            if (dialogueLines == null || dialogueLines.Length == 0) return;

            ShowLine(dialogueLines[currentLineIndex]);

            currentLineIndex++;

            if (currentLineIndex >= dialogueLines.Length)
                currentLineIndex = loopDialogue ? 0 : dialogueLines.Length - 1;
        }

      
        public void ShowLine(string text)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
                
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            currentCoroutine = StartCoroutine(ShowRoutine(text));
        }

        public void Hide(bool instant = false)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);
                
            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            if (instant || !gameObject.activeInHierarchy)
            {
                isVisible = false;
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                if (bubbleRoot  != null) bubbleRoot.SetActive(false);
                return;
            }

            currentCoroutine = StartCoroutine(HideRoutine());
        }

      
        public void ResetDialogue() => currentLineIndex = 0;


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