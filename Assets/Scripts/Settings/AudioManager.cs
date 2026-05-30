using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("[ МИКШЕР ]")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string     musicVolumeParam  = "MusicVolume";
    [SerializeField] private string     sfxVolumeParam    = "SFXVolume";

    [Header("[ ИСТОЧНИКИ ЗВУКА ]")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource streetAmbientSource;
    [SerializeField] private AudioSource indoorAmbientSource;

    [Header("[ НАСТРОЙКИ ]")]
    [SerializeField] private float fadeDuration     = 1.5f;
    [SerializeField] private float maxAmbientVolume = 0.8f;

    private bool      isCurrentlyIndoor = false;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupSources();
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
        // Применяем сохранённые настройки через миксер
        ApplyMixerVolume(musicVolumeParam,  PlayerPrefs.GetFloat("MusicVolume", 0.8f));
        ApplyMixerVolume(sfxVolumeParam,    PlayerPrefs.GetFloat("SFXVolume",   1.0f));
    }

    private void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
            StopGameplayAudio();
    }

    private void SetupSources()
    {
        if (musicSource != null)
        {
            musicSource.loop        = true;
            musicSource.playOnAwake = false;
            musicSource.volume      = 1f; // громкость через миксер
            musicSource.Play();
        }

        if (streetAmbientSource != null)
        {
            streetAmbientSource.loop        = true;
            streetAmbientSource.playOnAwake = false;
            streetAmbientSource.volume      = maxAmbientVolume;
            streetAmbientSource.Play();
        }

        if (indoorAmbientSource != null)
        {
            indoorAmbientSource.loop        = true;
            indoorAmbientSource.playOnAwake = false;
            indoorAmbientSource.volume      = 0f;
            indoorAmbientSource.Play();
        }
    }

    private void StopGameplayAudio()
    {
        if (streetAmbientSource != null) streetAmbientSource.Stop();
        if (indoorAmbientSource != null) indoorAmbientSource.Stop();
    }

    // ─── ГРОМКОСТЬ ЧЕРЕЗ МИКСЕР ───────────────────────────────────────────

    /// <summary>
    /// Установить громкость музыки (0-1). Вызывается из SettingsManager.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        ApplyMixerVolume(musicVolumeParam, volume);
    }

    /// <summary>
    /// Установить громкость SFX (0-1). Вызывается из SettingsManager.
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        ApplyMixerVolume(sfxVolumeParam, volume);
    }

    private void ApplyMixerVolume(string param, float linearValue)
    {
        if (mainMixer == null) return;

        // Переводим линейное значение (0-1) в децибелы
        float db = linearValue > 0.0001f
            ? Mathf.Log10(linearValue) * 20f
            : -80f;

        mainMixer.SetFloat(param, db);
    }

    // GetSFXVolume больше не нужен — громкость контролирует миксер
    // Оставляем для совместимости со старым кодом
    public float GetSFXVolume()   => 1f;
    public float GetMusicVolume() => 1f;

    // ─── ЗОНЫ ────────────────────────────────────────────────────────────

    public void ChangeZone(bool isIndoor)
    {
        if (this == null) return;
        isCurrentlyIndoor = isIndoor;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeAmbient(isIndoor));
    }

    private IEnumerator FadeAmbient(bool isIndoor)
    {
        float time        = 0f;
        float startStreet = streetAmbientSource != null ? streetAmbientSource.volume : 0f;
        float startIndoor = indoorAmbientSource != null ? indoorAmbientSource.volume : 0f;

        float targetStreet = isIndoor ? 0f : maxAmbientVolume;
        float targetIndoor = isIndoor ? maxAmbientVolume : 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);

            if (streetAmbientSource != null)
                streetAmbientSource.volume = Mathf.Lerp(startStreet, targetStreet, t);
            if (indoorAmbientSource != null)
                indoorAmbientSource.volume = Mathf.Lerp(startIndoor, targetIndoor, t);

            yield return null;
        }

        if (streetAmbientSource != null) streetAmbientSource.volume = targetStreet;
        if (indoorAmbientSource != null) indoorAmbientSource.volume = targetIndoor;

        fadeRoutine = null;
    }
}