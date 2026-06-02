using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.UI
{
    public class SettingsManager : MonoBehaviour
    {
        // ─── CONFIGURATION ────────────────────────────────────────────────
        [Header("[ КЛЮЧИ PLAYERPREFS ]")]
        [SerializeField] private string musicVolumeKey = "MusicVolume";
        [SerializeField] private string sfxVolumeKey   = "SFXVolume";
        [SerializeField] private string qualityKey     = "QualityLevel";
        [SerializeField] private string fullscreenKey  = "Fullscreen";
        [SerializeField] private string resolutionKey  = "Resolution";
        [SerializeField] private string shadowsKey     = "ShadowQuality";
        [SerializeField] private string fpsLimitKey    = "FPSLimit";
        [SerializeField] private string vsyncKey       = "VSync";
        // ─────────────────────────────────────────────────────────────────

        [Header("[ ЗВУК ]")]
        [SerializeField] private Slider          musicVolumeSlider;
        [SerializeField] private Slider          sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicValueText;
        [SerializeField] private TextMeshProUGUI sfxValueText;

        [Header("[ ГРАФИКА ]")]
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown shadowsDropdown;
        [SerializeField] private TMP_Dropdown fpsDropdown;
        [SerializeField] private Toggle       fullscreenToggle;
        [SerializeField] private Toggle       vsyncToggle;

        [Header("[ UI ПАНЕЛИ ]")]
        [SerializeField] private GameObject settingsPanelObject;
        [SerializeField] private GameObject confirmationPopupObject;
        [SerializeField] private GameObject pauseMenuObject;

        private Resolution[] availableResolutions;

        private int   currentResolutionIndex;
        private int   currentQualityIndex;
        private int   currentShadowIndex;
        private int   currentFPSIndex;
        private float currentMusicVolume;
        private float currentSFXVolume;
        private bool  currentFullscreen;
        private bool  currentVSync;

        private bool  initialized;
        private bool  isLoading;

        private readonly int[] fpsOptions = { 30, 60, 120, 144, 240, 0 };

        // ─── ЖИЗНЕННЫЙ ЦИКЛ ──────────────────────────────────────────────

        private void Awake()
        {
            // Инициализируем сразу в Awake — до того как PausePanel скроется
            EnsureInitialized();

            if (settingsPanelObject     != null) settingsPanelObject.SetActive(false);
            if (confirmationPopupObject != null) confirmationPopupObject.SetActive(false);
        }

        private void Start()
        {
            // Start оставляем пустым — всё делает Awake
        }

        /// <summary>
        /// Вызывай перед OpenSettingsPanel если не уверен что инициализация прошла
        /// </summary>
        public void EnsureInitialized()
        {
            if (initialized) return;

            SetupResolutions();
            SetupQuality();
            SetupShadows();
            SetupFPS();
            BindControls();
            initialized = true;
        }

        private void Update()
        {
            if (confirmationPopupObject != null &&
                confirmationPopupObject.activeSelf &&
                Input.GetKeyDown(KeyCode.Escape))
            {
                confirmationPopupObject.SetActive(false);
            }
        }

        // ─── ОТКРЫТИЕ / ЗАКРЫТИЕ ─────────────────────────────────────────

        public void OpenSettingsPanel()
        {
            if (settingsPanelObject == null)
                return;

            if (confirmationPopupObject != null)
                confirmationPopupObject.SetActive(false);

            if (pauseMenuObject != null)
                pauseMenuObject.SetActive(false);

            LoadSettings();

            settingsPanelObject.SetActive(true);
        }

        public void AttemptCloseSettings()
        {
            if (confirmationPopupObject != null)
            {
                confirmationPopupObject.SetActive(true);
            }
            else
            {
                ForceCloseWithoutSaving();
            }
        }

        public void ConfirmSaveAndClose()
        {
            ApplyAndSave();
            CloseAll();
        }

        public void ConfirmDiscardAndClose()
        {
            LoadSettings();
            CloseAll();
        }

        private void ForceCloseWithoutSaving()
        {
            LoadSettings();
            CloseAll();
        }

        private void CloseAll()
        {
            if (confirmationPopupObject != null)
                confirmationPopupObject.SetActive(false);

            if (settingsPanelObject != null)
                settingsPanelObject.SetActive(false);

            if (pauseMenuObject != null)
                pauseMenuObject.SetActive(true);
        }

        // ─── ЗАГРУЗКА НАСТРОЕК В UI ───────────────────────────────────────

        private void LoadSettings()
        {
            isLoading = true;

            currentMusicVolume  = PlayerPrefs.GetFloat(musicVolumeKey, 0.8f);
            currentSFXVolume    = PlayerPrefs.GetFloat(sfxVolumeKey,   1.0f);
            currentFullscreen   = PlayerPrefs.GetInt(fullscreenKey, 1) == 1;
            currentVSync        = PlayerPrefs.GetInt(vsyncKey, 1) == 1;
            currentShadowIndex  = PlayerPrefs.GetInt(shadowsKey, 2);
            currentFPSIndex     = PlayerPrefs.GetInt(fpsLimitKey, 1);
            currentQualityIndex = PlayerPrefs.GetInt(qualityKey, QualitySettings.GetQualityLevel());

            if (musicVolumeSlider  != null) { musicVolumeSlider.value  = currentMusicVolume; UpdateMusicText(currentMusicVolume); }
            if (sfxVolumeSlider    != null) { sfxVolumeSlider.value    = currentSFXVolume;   UpdateSFXText(currentSFXVolume); }
            if (fullscreenToggle   != null)   fullscreenToggle.isOn    = currentFullscreen;
            if (vsyncToggle        != null)   vsyncToggle.isOn         = currentVSync;
            if (qualityDropdown    != null)   qualityDropdown.value    = currentQualityIndex;
            if (shadowsDropdown    != null)   shadowsDropdown.value    = currentShadowIndex;
            if (fpsDropdown        != null)   fpsDropdown.value        = currentFPSIndex;

            if (resolutionDropdown != null && availableResolutions != null)
            {
                currentResolutionIndex = PlayerPrefs.GetInt(resolutionKey, 0);
                currentResolutionIndex = Mathf.Clamp(currentResolutionIndex, 0, availableResolutions.Length - 1);
                resolutionDropdown.value = currentResolutionIndex;
            }

            // Применяем системные настройки
            ApplyAllSystemSettings();

            isLoading = false;
        }

        // ─── ПРИВЯЗКА ЛИСТЕНЕРОВ ─────────────────────────────────────────

        private void BindControls()
        {
            musicVolumeSlider?.onValueChanged.AddListener(v =>
            {
                if (isLoading) return;

                currentMusicVolume = v;
                UpdateMusicText(v);
                AudioManager.Instance?.SetMusicVolume(v);
            });

            sfxVolumeSlider?.onValueChanged.AddListener(v =>
            {
                if (isLoading) return;

                currentSFXVolume = v;
                UpdateSFXText(v);
                AudioManager.Instance?.SetSFXVolume(v);
            });

            qualityDropdown?.onValueChanged.AddListener(v =>
            {
                if (isLoading) return;
                currentQualityIndex = v;
            });

            resolutionDropdown?.onValueChanged.AddListener(v =>
            {
                if (isLoading) return;
                currentResolutionIndex = v;
            });

            shadowsDropdown?.onValueChanged.AddListener(v =>
            {
                if (isLoading) return;
                currentShadowIndex = v;
            });

            fpsDropdown?.onValueChanged.AddListener(v =>
            {
                if (isLoading) return;
                currentFPSIndex = v;
            });

            fullscreenToggle?.onValueChanged.AddListener(v =>
            {
                if (isLoading) return;
                currentFullscreen = v;
            });

            vsyncToggle?.onValueChanged.AddListener(v =>
            {
                if (isLoading) return;
                currentVSync = v;
            });
        }

        // ─── СОХРАНЕНИЕ И ПРИМЕНЕНИЕ ─────────────────────────────────────

        public void ApplyAndSavePublic() => ApplyAndSave();

        private void ApplyAndSave()
        {
            PlayerPrefs.SetFloat(musicVolumeKey, currentMusicVolume);
            PlayerPrefs.SetFloat(sfxVolumeKey,   currentSFXVolume);
            PlayerPrefs.SetInt(qualityKey,        currentQualityIndex);
            PlayerPrefs.SetInt(resolutionKey,     currentResolutionIndex);
            PlayerPrefs.SetInt(fullscreenKey,     currentFullscreen ? 1 : 0);
            PlayerPrefs.SetInt(vsyncKey,          currentVSync ? 1 : 0);
            PlayerPrefs.SetInt(shadowsKey,        currentShadowIndex);
            PlayerPrefs.SetInt(fpsLimitKey,       currentFPSIndex);
            PlayerPrefs.Save();

            ApplyAllSystemSettings();
        }

        private void ApplyAllSystemSettings()
        {
            AudioManager.Instance?.SetMusicVolume(currentMusicVolume);
            AudioManager.Instance?.SetSFXVolume(currentSFXVolume);

            if (currentQualityIndex >= 0 && currentQualityIndex < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(currentQualityIndex, true);

            if (availableResolutions != null &&
                currentResolutionIndex >= 0 &&
                currentResolutionIndex < availableResolutions.Length)
            {
                Resolution r = availableResolutions[currentResolutionIndex];
                Screen.SetResolution(r.width, r.height, currentFullscreen);
            }

            Screen.fullScreen = currentFullscreen;
            ApplyShadows(currentShadowIndex);
            ApplyFPS(currentFPSIndex);
            QualitySettings.vSyncCount = currentVSync ? 1 : 0;
        }

        private void ApplyShadows(int index)
        {
            switch (index)
            {
                case 0:
                    QualitySettings.shadows = ShadowQuality.Disable;
                    break;
                case 1:
                    QualitySettings.shadows          = ShadowQuality.HardOnly;
                    QualitySettings.shadowDistance   = 20f;
                    QualitySettings.shadowResolution = ShadowResolution.Low;
                    break;
                case 2:
                    QualitySettings.shadows          = ShadowQuality.All;
                    QualitySettings.shadowDistance   = 40f;
                    QualitySettings.shadowResolution = ShadowResolution.Medium;
                    break;
                case 3:
                    QualitySettings.shadows          = ShadowQuality.All;
                    QualitySettings.shadowDistance   = 80f;
                    QualitySettings.shadowResolution = ShadowResolution.High;
                    break;
            }
        }

        private void ApplyFPS(int index)
        {
            if (index >= 0 && index < fpsOptions.Length)
                Application.targetFrameRate = fpsOptions[index];
        }

        // ─── ДРОПДАУНЫ ───────────────────────────────────────────────────

        private void SetupResolutions()
        {
            if (resolutionDropdown == null) return;

            resolutionDropdown.ClearOptions();

            var seen     = new System.Collections.Generic.HashSet<string>();
            var filtered = new System.Collections.Generic.List<Resolution>();

            foreach (Resolution r in Screen.resolutions)
            {
                string key = $"{r.width}x{r.height}";
                if (seen.Add(key)) filtered.Add(r);
            }

            availableResolutions = filtered.ToArray();

            int currentIndex = 0;
            var options      = new System.Collections.Generic.List<string>();

            for (int i = 0; i < availableResolutions.Length; i++)
            {
                Resolution r = availableResolutions[i];
                options.Add($"{r.width} × {r.height}");

                if (r.width  == Screen.currentResolution.width &&
                    r.height == Screen.currentResolution.height)
                    currentIndex = i;
            }

            resolutionDropdown.AddOptions(options);
            currentResolutionIndex   = PlayerPrefs.GetInt(resolutionKey, currentIndex);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }

        private void SetupQuality()
        {
            if (qualityDropdown == null) return;
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(
                new System.Collections.Generic.List<string>(QualitySettings.names)
            );
            qualityDropdown.RefreshShownValue();
        }

        private void SetupShadows()
        {
            if (shadowsDropdown == null) return;
            shadowsDropdown.ClearOptions();
            shadowsDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Выключены", "Низкое", "Среднее", "Высокое"
            });
            shadowsDropdown.RefreshShownValue();
        }

        private void SetupFPS()
        {
            if (fpsDropdown == null) return;
            fpsDropdown.ClearOptions();
            fpsDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "30 FPS", "60 FPS", "120 FPS", "144 FPS", "240 FPS", "Без ограничений"
            });
            fpsDropdown.RefreshShownValue();
        }

        // ─── ТЕКСТ ───────────────────────────────────────────────────────

        private void UpdateMusicText(float v)
        {
            if (musicValueText != null)
                musicValueText.text = Mathf.RoundToInt(v * 100f) + "%";
        }

        private void UpdateSFXText(float v)
        {
            if (sfxValueText != null)
                sfxValueText.text = Mathf.RoundToInt(v * 100f) + "%";
        }

        private void OnDestroy()
        {
            musicVolumeSlider?.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider?.onValueChanged.RemoveAllListeners();
            qualityDropdown?.onValueChanged.RemoveAllListeners();
            resolutionDropdown?.onValueChanged.RemoveAllListeners();
            shadowsDropdown?.onValueChanged.RemoveAllListeners();
            fpsDropdown?.onValueChanged.RemoveAllListeners();
            fullscreenToggle?.onValueChanged.RemoveAllListeners();
            vsyncToggle?.onValueChanged.RemoveAllListeners();
        }
    }
}