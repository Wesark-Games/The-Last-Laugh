using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.Audio;
using Project.SaveSystem;

namespace Project.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        // ─── CONFIGURATION ───────────────────────────────────────────────────
        [Header("[ НАЗВАНИЯ СЦЕН ]")]
        [SerializeField] private string firstGameSceneName = "Prologue";

        [Header("[ ФОН ]")]
        [SerializeField] private VideoPlayer backgroundVideoPlayer;
        [SerializeField] private Image       backgroundImage;
        [SerializeField] private Sprite      noirBackgroundSprite;
        [SerializeField] private Color       overlayColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Image       overlayImage;

        [Header("[ АНИМАЦИЯ ]")]
        [SerializeField] private Animator menuAnimator;
        [SerializeField] private string   showTrigger = "Show";

        [Header("[ ЗВУК ]")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip   menuMusic;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.4f;

        [Header("[ АУДИОМИКШЕР ]")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string     gameMixerVolumeParam = "GameVolume";

        [Header("[ КНОПКИ ]")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("[ НАСТРОЙКИ (панель поверх меню) ]")]
        [Tooltip("Объект панели настроек — скопирован из PausePanel")]
        [SerializeField] private GameObject settingsPanelRoot;
        [Tooltip("Главное меню — скрывается когда открыты настройки")]
        [SerializeField] private GameObject mainMenuRoot;
        [Tooltip("Картинка которая появляется за панелью настроек")]
        [SerializeField] private Image   settingsBackgroundImage;
        [SerializeField] private Sprite  settingsBackgroundSprite;
        [SerializeField] private float   settingsBgFadeDuration = 0.4f;

        private Coroutine settingsBgCoroutine;

        private SettingsManager settingsManager;

        private void Start()
        {
            Time.timeScale = 1f;

            SetupBackground();
            SetupMusic();
            SetupContinueButton();
            BindButtons();
            SetupSettingsPanel();

            if (menuAnimator != null)
                menuAnimator.SetTrigger(showTrigger);
        }

        // ─── НАСТРОЙКИ ───────────────────────────────────────────────────────

        private void SetupSettingsPanel()
        {
            if (settingsPanelRoot == null) return;

            // Ищем SettingsManager внутри панели
            settingsManager = settingsPanelRoot.GetComponentInChildren<SettingsManager>(true);

            if (settingsManager != null)
                settingsManager.EnsureInitialized();

            settingsPanelRoot.SetActive(false);
        }

        private void OnSettingsClicked()
{
    if (settingsManager == null) return;

    if (mainMenuRoot != null)
        mainMenuRoot.SetActive(false);

    // Показываем фон настроек
    if (settingsBackgroundImage != null && settingsBackgroundSprite != null)
        settingsBackgroundImage.sprite = settingsBackgroundSprite;

    settingsPanelRoot.SetActive(true);
    settingsManager.OpenSettingsPanel();

    StartSettingsBgFade(0f, 1f);
}

        /// <summary>
        /// Вызывается из SettingsManager.CloseAll() через pauseMenuObject
        /// </summary>
       public void ShowMainMenu()
{
    // Плавно скрываем фон настроек, потом показываем меню
    if (settingsBgCoroutine != null) StopCoroutine(settingsBgCoroutine);
    settingsBgCoroutine = StartCoroutine(HideSettingsBgThenShowMenu());
}

        // ─── ОСТАЛЬНОЕ ───────────────────────────────────────────────────────

        private void SetupContinueButton()
        {
            if (continueButton == null) return;
            bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasSave();
            continueButton.gameObject.SetActive(hasSave);
        }

        private void SetupBackground()
        {
            if (backgroundVideoPlayer != null)
            {
                backgroundVideoPlayer.isLooping = true;
                backgroundVideoPlayer.Play();
            }
            else if (backgroundImage != null && noirBackgroundSprite != null)
            {
                backgroundImage.sprite = noirBackgroundSprite;
            }

            if (overlayImage != null)
                overlayImage.color = overlayColor;
        }

        private void SetupMusic()
        {
            if (musicSource == null || menuMusic == null) return;
            musicSource.clip   = menuMusic;
            musicSource.volume = musicVolume;
            musicSource.loop   = true;
            musicSource.Play();

            if (audioMixer != null)
            {
                float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
                float db = savedMusic <= 0f ? -80f : Mathf.Log10(savedMusic) * 20f;
                audioMixer.SetFloat(gameMixerVolumeParam, db);
            }
        }

        private void BindButtons()
        {
            newGameButton?.onClick.AddListener(OnNewGameClicked);
            continueButton?.onClick.AddListener(OnContinueClicked);
            settingsButton?.onClick.AddListener(OnSettingsClicked);
            quitButton?.onClick.AddListener(OnQuitClicked);
        }

        private void OnNewGameClicked()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.IsLoadingSave = false;
                SaveManager.Instance.DeleteSave();
            }

            LoadingScreenManager.LoadSceneWithoutLoadingItDirectly(firstGameSceneName);

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene("LoadingScene");
            else
                SceneManager.LoadScene("LoadingScene");
        }

        private void OnContinueClicked()
        {
            if (SaveManager.Instance == null) return;

            if (SaveManager.Instance.Load())
            {
                SaveManager.Instance.IsLoadingSave = true;

                string targetScene = SaveManager.Instance.Data.currentScene;
                if (string.IsNullOrEmpty(targetScene)) targetScene = firstGameSceneName;

                LoadingScreenManager.LoadSceneWithoutLoadingItDirectly(targetScene);

                if (SceneTransitionManager.Instance != null)
                    SceneTransitionManager.Instance.LoadScene("LoadingScene");
                else
                    SceneManager.LoadScene("LoadingScene");
            }
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

private void StartSettingsBgFade(float from, float to)
{
    if (settingsBackgroundImage == null) return;
    if (settingsBgCoroutine != null) StopCoroutine(settingsBgCoroutine);
    settingsBgCoroutine = StartCoroutine(FadeSettingsBg(from, to));
}

private System.Collections.IEnumerator FadeSettingsBg(float from, float to)
{
    if (settingsBackgroundImage == null) yield break;

    settingsBackgroundImage.gameObject.SetActive(true);

    Color c = settingsBackgroundImage.color;
    float t = 0f;

    while (t < 1f)
    {
        t += Time.unscaledDeltaTime / settingsBgFadeDuration;
        c.a = Mathf.Lerp(from, to, t * t * (3f - 2f * t));
        settingsBackgroundImage.color = c;
        yield return null;
    }

    c.a = to;
    settingsBackgroundImage.color = c;

    if (to <= 0f)
        settingsBackgroundImage.gameObject.SetActive(false);
}

private System.Collections.IEnumerator HideSettingsBgThenShowMenu()
{
    // Сначала плавно скрываем фон настроек
    yield return StartCoroutine(FadeSettingsBg(1f, 0f));

    if (settingsPanelRoot != null)
        settingsPanelRoot.SetActive(false);

    if (mainMenuRoot != null)
        mainMenuRoot.SetActive(true);
}

        private void OnDestroy()
        {
            newGameButton?.onClick.RemoveAllListeners();
            continueButton?.onClick.RemoveAllListeners();
            settingsButton?.onClick.RemoveAllListeners();
            quitButton?.onClick.RemoveAllListeners();
            
        }
    }
}