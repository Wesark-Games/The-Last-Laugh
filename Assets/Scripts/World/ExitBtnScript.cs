using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Project.World
{
    /// <summary>
    /// Выход на арену. При входе в триггер:
    ///   - Фризит игрока
    ///   - Показывает анимированную плашку с кнопками Да / Нет
    ///   - Да → переход в другую сцену
    ///   - Нет → возвращает управление
    /// </summary>
    public class ExitPrompt : MonoBehaviour
    {
        // ─── CONFIGURATION ────────────────────────────────────────────────
        [Header("[ СЦЕНА ]")]
        [Tooltip("Сцена в которую переходим при нажатии Да")]
        [SerializeField] private string targetScene = "GameScene";
        [Tooltip("Использовать LoadingScene как промежуточную")]
        [SerializeField] private bool   useLoadingScene = true;

        [Header("[ UI ПЛАШКА ]")]
        [Tooltip("Корневой объект плашки — выключен по умолчанию")]
        [SerializeField] private GameObject promptRoot;
        [Tooltip("Текст на плашке")]
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private string          promptMessage = "Выйти на арену?";

        [Header("[ КНОПКИ ]")]
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        [Header("[ АНИМАЦИЯ ПЛАШКИ ]")]
        [SerializeField] private float slideInDuration  = 0.4f;
        [SerializeField] private float slideOutDuration = 0.3f;
        [Tooltip("Смещение по Y с которого приезжает плашка")]
        [SerializeField] private float slideOffsetY = -150f;

        [Header("[ ИГРОК ]")]
        [SerializeField] private string playerTag = "Player";
        // ─────────────────────────────────────────────────────────────────

        private RectTransform       promptRect;
        private CanvasGroup         promptCanvasGroup;
        private Vector2             promptOriginalPos;
        private Coroutine           animCoroutine;

        private Project.Player.PlayerController playerController;
        private bool                            isPromptOpen;

        private void Awake()
        {
            if (promptRoot != null)
            {
                promptRect        = promptRoot.GetComponent<RectTransform>();
                promptCanvasGroup = promptRoot.GetComponent<CanvasGroup>();

                if (promptCanvasGroup == null)
                    promptCanvasGroup = promptRoot.AddComponent<CanvasGroup>();

                if (promptRect != null)
                    promptOriginalPos = promptRect.anchoredPosition;

                promptRoot.SetActive(false);
            }

            if (promptText != null)
                promptText.text = promptMessage;
        }

        private void Start()
        {
            yesButton?.onClick.AddListener(OnYesClicked);
            noButton?.onClick.AddListener(OnNoClicked);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(playerTag)) return;
            if (isPromptOpen)                 return;

            playerController = other.GetComponent<Project.Player.PlayerController>();
            ShowPrompt();
        }

        // ─── ПОКАЗ / СКРЫТИЕ ─────────────────────────────────────────────

        private void ShowPrompt()
        {
            isPromptOpen = true;

            // Фризим игрока
            if (playerController != null)
                playerController.SetMovementEnabled(false);

            if (promptRoot == null) return;

            promptRoot.SetActive(true);
            promptCanvasGroup.interactable   = false;
            promptCanvasGroup.blocksRaycasts = false;

            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(SlideIn());
        }

        private void HidePrompt()
        {
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(SlideOut(() =>
            {
                isPromptOpen = false;

                // Возвращаем управление
                if (playerController != null)
                    playerController.SetMovementEnabled(true);
            }));
        }

        // ─── КНОПКИ ──────────────────────────────────────────────────────

        private void OnYesClicked()
        {
            promptCanvasGroup.interactable   = false;
            promptCanvasGroup.blocksRaycasts = false;

            if (useLoadingScene)
            {
                Project.UI.LoadingScreenManager.LoadSceneWithoutLoadingItDirectly(targetScene);

                if (Project.UI.SceneTransitionManager.Instance != null)
                    Project.UI.SceneTransitionManager.Instance.LoadScene("LoadingScene");
                else
                    SceneManager.LoadScene("LoadingScene");
            }
            else
            {
                if (Project.UI.SceneTransitionManager.Instance != null)
                    Project.UI.SceneTransitionManager.Instance.LoadScene(targetScene);
                else
                    SceneManager.LoadScene(targetScene);
            }
        }

        private void OnNoClicked()
        {
            HidePrompt();
        }

        // ─── АНИМАЦИИ ────────────────────────────────────────────────────

        private IEnumerator SlideIn()
        {
            if (promptRect == null) yield break;

            Vector2 startPos = promptOriginalPos + new Vector2(0f, slideOffsetY);
            promptRect.anchoredPosition = startPos;
            promptCanvasGroup.alpha     = 0f;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / slideInDuration;
                float eased = EaseOutCubic(Mathf.Clamp01(t));

                promptRect.anchoredPosition = Vector2.Lerp(startPos, promptOriginalPos, eased);
                promptCanvasGroup.alpha     = eased;

                yield return null;
            }

            promptRect.anchoredPosition      = promptOriginalPos;
            promptCanvasGroup.alpha          = 1f;
            promptCanvasGroup.interactable   = true;
            promptCanvasGroup.blocksRaycasts = true;
        }

        private IEnumerator SlideOut(System.Action onComplete)
        {
            if (promptRect == null) { onComplete?.Invoke(); yield break; }

            promptCanvasGroup.interactable   = false;
            promptCanvasGroup.blocksRaycasts = false;

            Vector2 endPos = promptOriginalPos + new Vector2(0f, slideOffsetY);
            float   t      = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / slideOutDuration;
                float eased = EaseInCubic(Mathf.Clamp01(t));

                promptRect.anchoredPosition = Vector2.Lerp(promptOriginalPos, endPos, eased);
                promptCanvasGroup.alpha     = Mathf.Lerp(1f, 0f, eased);

                yield return null;
            }

            promptRoot.SetActive(false);
            onComplete?.Invoke();
        }

        private float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        private float EaseInCubic(float t)  => t * t * t;

        private void OnDestroy()
        {
            yesButton?.onClick.RemoveAllListeners();
            noButton?.onClick.RemoveAllListeners();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            Gizmos.DrawCube(transform.position, new Vector3(2f, 2f, 0f));
            Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
            Gizmos.DrawWireCube(transform.position, new Vector3(2f, 2f, 0f));
        }
    }
}