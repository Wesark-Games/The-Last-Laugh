using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuAudioManager : MonoBehaviour
{
    public static MenuAudioManager Instance;

    [Header("[ МИКШЕР ]")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";

    [Header("[ ИСТОЧНИК ЗВУКА МЕНЮ ]")]
    [SerializeField] private AudioSource menuMusicSource;

    [Header("[ НАСТРОЙКИ ЗАТУХАНИЯ ]")]
    [SerializeField] private float fadeOutDuration = 1.0f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMenuMusic();
        }
        else
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Подгружаем сохраненные настройки громкости из PlayerPrefs
        LoadVolumeSettings();
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void InitializeMenuMusic()
    {
        if (menuMusicSource != null)
        {
            menuMusicSource.loop = true;
            menuMusicSource.playOnAwake = false;
            menuMusicSource.volume = 1f; // Реальная громкость регулируется микшером
            menuMusicSource.Play();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Если вышли на игровую сцену (например, Пролог или геймплей), глушим музыку меню
        if (scene.name != "SplashScene" && scene.name != "MainMenu" && scene.name != "LoadingScene")
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeOutAndStop());
        }
    }

    private IEnumerator FadeOutAndStop()
    {
        if (menuMusicSource == null) yield break;

        float startVolume = menuMusicSource.volume;
        float time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.unscaledDeltaTime;
            menuMusicSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeOutDuration);
            yield return null;
        }

        menuMusicSource.Stop();
        
        // Когда музыка полностью затихла, этот менеджер меню больше не нужен — уничтожаем его
        Destroy(gameObject);
    }

    // ─── УПРАВЛЕНИЕ ГРОМКОСТЬЮ (Вызывать из UI настроек) ───────────────────

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        ApplyMixerVolume(musicVolumeParam, volume);
    }

    public void SetSFXVolume(float volume)
    {
        PlayerPrefs.SetFloat("SFXVolume", volume);
        ApplyMixerVolume(sfxVolumeParam, volume);
    }

    private void ApplyMixerVolume(string param, float linearValue)
    {
        if (mainMixer == null) return;

        float db = linearValue > 0.0001f 
            ? Mathf.Log10(linearValue) * 20f 
            : -80f;

        mainMixer.SetFloat(param, db);
    }

    private void LoadVolumeSettings()
    {
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1.0f);

        ApplyMixerVolume(musicVolumeParam, savedMusic);
        ApplyMixerVolume(sfxVolumeParam, savedSFX);
    }
}