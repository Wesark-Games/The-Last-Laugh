using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Project.UI
{
    public class PauseManager : MonoBehaviour
    {
        // ─── CONFIGURATION ────────────────────────────────────────────────
        [Header("[ КЛАВИШИ ]")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        [Header("[ НАЗВАНИЯ СЦЕН ]")]
        // [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private AudioMixer audioMixer;

        [Header("[ АНИМАЦИЯ ОВЕРЛЕЯ ]")]
        [SerializeField] private float overlayFadeDuration = 0.3f;
        // ─────────────────────────────────────────────────────────────────

        [Header("[ UI ЭЛЕМЕНТЫ ]")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Image      overlayImage;

        [Header("[ КНОПКИ ПАУЗЫ ]")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("[ КНОПКИ НАСТРОЕК ]")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button backButton;

        public static PauseManager Instance { get; private set; }
        public bool IsPaused { get; private set; }

        private PauseAnimator  pauseAnimator;
        private SettingsManager settingsManager;
        private Coroutine      overlayCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (pausePanel != null)
            {
                pauseAnimator   = pausePanel.GetComponent<PauseAnimator>();
                settingsManager = pausePanel.GetComponentInChildren<SettingsManager>(true);
                pausePanel.SetActive(false);
            }

            if (settingsPanel != null)
                settingsPanel.SetActive(false);

            if (overlayImage != null)
                SetOverlayAlpha(0f);

            BindButtons();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }

        private void Update()
{
    if (Input.GetKeyDown(pauseKey))
    {
        if (!IsPaused)
        {
            Pause();
            return;
        }

        // Если открыты настройки — передаём Escape в SettingsManager
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            if (settingsManager != null)
                settingsManager.AttemptCloseSettings();
            return;
        }

        Resume();
    }
}

        private void BindButtons()
        {
            resumeButton?.onClick.AddListener(Resume);
            settingsButton?.onClick.AddListener(OpenSettings);
            mainMenuButton?.onClick.AddListener(GoToMainMenu);
            applyButton?.onClick.AddListener(ApplySettings);
            backButton?.onClick.AddListener(ShowPauseMenu);
        }

        // ─── ПАУЗА ───────────────────────────────────────────────────────

        public void TogglePause()
        {
            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            IsPaused       = true;
            Time.timeScale = 0f;

            if (pausePanel != null)
                pausePanel.SetActive(true);

            // Показываем меню паузы, скрываем настройки
            ShowPauseMenu();

            FadeOverlay(0f, 1f);
            pauseAnimator?.Show();
        }

        public void Resume()
        {
            IsPaused = false;

            FadeOverlay(1f, 0f);

            if (pauseAnimator != null)
            {
                pauseAnimator.Hide(() =>
                {
                    Time.timeScale = 1f;
                    if (pausePanel != null)
                        pausePanel.SetActive(false);
                });
            }
            else
            {
                Time.timeScale = 1f;
                if (pausePanel != null)
                    pausePanel.SetActive(false);
            }
        }

        // ─── НАВИГАЦИЯ МЕЖДУ ПАНЕЛЯМИ ─────────────────────────────────────

        /// <summary>
        /// Показать основное меню паузы
        /// </summary>
        public void ShowPauseMenu()
        {
            if (pauseMenu     != null) pauseMenu.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        /// <summary>
        /// Открыть настройки внутри паузы
        /// </summary>
        public void OpenSettings()
{
    if (settingsManager != null)
    {
        // Инициализируем если ещё не было
        settingsManager.EnsureInitialized();
        settingsManager.OpenSettingsPanel();
    }
    else
    {
        // Fallback если settingsManager не найден через GetComponentInChildren
        if (pauseMenu     != null) pauseMenu.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
}

        private void ApplySettings()
        {
            if (settingsManager != null)
                settingsManager.ApplyAndSavePublic();
        }

        // ─── ГЛАВНОЕ МЕНЮ ────────────────────────────────────────────────

        public void GoToMainMenu()
        {
            // Жестко тушим все AudioSource на сцене, чтобы они не лезли в меню
            AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (AudioSource source in allAudioSources)
            {
                source.Stop();
            }

            // Глушим микшер
            if (audioMixer != null)
            {
                audioMixer.SetFloat("GameVolume", -80f);
            }

            Time.timeScale = 1f;

            // Запоминаем, что после экрана загрузки надо открыть Главное меню
            Project.UI.LoadingScreenManager.LoadSceneWithoutLoadingItDirectly("MainMenu");

            // Запускаем плавный переход в LoadingScene через твой менеджер
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene("LoadingScene");
            else
                SceneManager.LoadScene("LoadingScene");
        }

        // ─── АНИМАЦИЯ ОВЕРЛЕЯ ────────────────────────────────────────────

        private void FadeOverlay(float from, float to)
        {
            if (overlayImage == null) return;
            if (overlayCoroutine != null)
                StopCoroutine(overlayCoroutine);
            overlayCoroutine = StartCoroutine(FadeRoutine(from, to));
        }

        private System.Collections.IEnumerator FadeRoutine(float from, float to)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / overlayFadeDuration;
                SetOverlayAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(t)));
                yield return null;
            }
            SetOverlayAlpha(to);
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (overlayImage == null) return;
            Color c = overlayImage.color;
            c.a = alpha;
            overlayImage.color = c;
        }

        private void OnDestroy()
        {
            resumeButton?.onClick.RemoveAllListeners();
            settingsButton?.onClick.RemoveAllListeners();
            mainMenuButton?.onClick.RemoveAllListeners();
            applyButton?.onClick.RemoveAllListeners();
            backButton?.onClick.RemoveAllListeners();
            Time.timeScale = 1f;
        }
    }
}