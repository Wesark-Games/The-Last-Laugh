using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro; 
using Project.SaveSystem;

namespace Project.UI
{
    public class PauseManager : MonoBehaviour
    {
        [Header("[ КЛАВИШИ ]")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        [Header("[ НАЗВАНИЯ СЦЕН ]")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("[ АНИМАЦИЯ ОВЕРЛЕЯ ]")]
        [SerializeField] private float overlayFadeDuration = 0.3f;

        [Header("[ UI ЭЛЕМЕНТЫ ]")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Image      overlayImage;

        [Header("[ ТЕКСТ ЗАДАНИЯ В ПАУЗЕ ]")]
        [SerializeField] private TextMeshProUGUI pauseQuestText; 

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
                pauseAnimator = pausePanel.GetComponent<PauseAnimator>();
                pausePanel.SetActive(false);
            }

            if (settingsPanel != null)
            {
                settingsManager = settingsPanel.GetComponentInChildren<SettingsManager>(true);
                
                // Инициализируем настройки и загружаем сохранения
                settingsManager?.EnsureInitialized();
                settingsManager?.AttemptCloseSettings();

                settingsPanel.SetActive(false);
            }

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
            backButton?.onClick.AddListener(OnBackFromSettings);
        }
        
        private void OnBackFromSettings()
        {
            if (settingsManager != null)
                settingsManager.AttemptCloseSettings();
            else
                ShowPauseMenu();
        }

        

        public void TogglePause()
        {
            if (IsPaused) Resume();
            else Pause();
        }

        public void Pause()
        {
            IsPaused       = true;
            Time.timeScale = 0f;

            UpdatePauseQuestText();

            if (pausePanel != null)
                pausePanel.SetActive(true);

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

        private void UpdatePauseQuestText()
        {
            if (pauseQuestText == null) return;

            if (SaveManager.Instance != null && !string.IsNullOrEmpty(SaveManager.Instance.Data.activeQuestText))
            {
                pauseQuestText.text = SaveManager.Instance.Data.activeQuestText;
            }
            else
            {
                pauseQuestText.text = "Нет активных заданий"; 
            }
        }



        public void ShowPauseMenu()
        {
            if (pauseMenu     != null) pauseMenu.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void OpenSettings()
        {
            if (settingsManager != null)
            {
                if (pauseMenu != null) pauseMenu.SetActive(false);
                
        
                if (settingsPanel != null) settingsPanel.SetActive(true);
                
                settingsManager.OpenSettingsPanel();
            }
        }

        private void ApplySettings()
        {
            if (settingsManager != null)
                settingsManager.ApplyAndSavePublic();
        }

      

        public void GoToMainMenu()
        {
            AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            foreach (AudioSource source in allAudioSources)
            {
                source.Stop();
            }

            if (audioMixer != null)
            {
                audioMixer.SetFloat("GameVolume", -80f);
            }

            Time.timeScale = 1f;

            Project.UI.LoadingScreenManager.LoadSceneWithoutLoadingItDirectly("MainMenu");

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene("LoadingScene");
            else
                SceneManager.LoadScene("LoadingScene");
        }


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