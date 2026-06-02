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
        [Tooltip("Имя сцены, с которой начинается сама игра (например, Prologue)")]
        [SerializeField] private string firstGameSceneName = "Prologue";
        [SerializeField] private string settingsSceneName = "SettingsScene";

        [Header("[ ФОН ]")]
        [SerializeField] private VideoPlayer backgroundVideoPlayer;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite noirBackgroundSprite;
        [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] private Image overlayImage;

        [Header("[ АНИМАЦИЯ ]")]
        [SerializeField] private Animator menuAnimator;
        [SerializeField] private string showTrigger = "Show";

        [Header("[ ЗВУК ]")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip menuMusic;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.4f;

        [Header("[ АУДИОМИКШЕР ]")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string gameMixerVolumeParam = "GameVolume";

        [Header("[ КНОПКИ ]")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            Time.timeScale = 1f;

            SetupBackground();
            SetupMusic();
            SetupContinueButton();
            BindButtons();

            if (menuAnimator != null)
                menuAnimator.SetTrigger(showTrigger);
        }

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
            if (SaveManager.Instance != null)
            {
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
        }

        private void OnSettingsClicked()
        {
            PlayerPrefs.SetString("PreviousScene", "MainMenu");
            PlayerPrefs.Save();

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(settingsSceneName);
            else
                SceneManager.LoadScene(settingsSceneName);
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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