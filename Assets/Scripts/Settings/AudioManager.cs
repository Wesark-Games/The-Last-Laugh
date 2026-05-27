using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource streetAmbientSource;
    [SerializeField] private AudioSource indoorAmbientSource;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float maxAmbientVolume = 0.5f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        SetupSources();
    }

    private void SetupSources()
    {
        musicSource.loop = true;
        musicSource.playOnAwake = true;
        musicSource.Play();

        streetAmbientSource.loop = true;
        streetAmbientSource.playOnAwake = true;
        streetAmbientSource.volume = maxAmbientVolume;
        streetAmbientSource.Play();

        indoorAmbientSource.loop = true;
        indoorAmbientSource.playOnAwake = true;
        indoorAmbientSource.volume = 0f;
        indoorAmbientSource.Play();
    }

    public void ChangeZone(bool isIndoor)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeAmbient(isIndoor));
    }

    private IEnumerator FadeAmbient(bool isIndoor)
    {
        float time = 0;
        float startStreet = streetAmbientSource.volume;
        float startIndoor = indoorAmbientSource.volume;

        float targetStreet = isIndoor ? 0f : maxAmbientVolume;
        float targetIndoor = isIndoor ? maxAmbientVolume : 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            streetAmbientSource.volume = Mathf.Lerp(startStreet, targetStreet, t);
            indoorAmbientSource.volume = Mathf.Lerp(startIndoor, targetIndoor, t);
            yield return null;
        }

        streetAmbientSource.volume = targetStreet;
        indoorAmbientSource.volume = targetIndoor;
    }
}