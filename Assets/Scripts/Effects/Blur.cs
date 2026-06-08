using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class Blur : MonoBehaviour
{
    [Header("[ ССЫЛКИ ]")]
    [SerializeField] private Volume blurVolume;

    [Header("[ НАСТРОЙКИ ]")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine blurCoroutine;

    private void Start()
    {
        if (blurVolume != null)
        {
            blurVolume.weight = 0f;
            blurVolume.gameObject.SetActive(false);
        }
    }

    public void BlurIn()
    {
        if (blurVolume != null)
        {
            blurVolume.gameObject.SetActive(true);
        }

        if (blurCoroutine != null) StopCoroutine(blurCoroutine);
        blurCoroutine = StartCoroutine(FadeBlurRoutine(true));
    }

    public void BlurOut()
    {
        if (blurCoroutine != null) StopCoroutine(blurCoroutine);
        blurCoroutine = StartCoroutine(FadeBlurRoutine(false));
    }

    private IEnumerator FadeBlurRoutine(bool fadeIn)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / fadeDuration; 
            float progress = Mathf.Clamp01(t);

            if (blurVolume != null)
            {
                blurVolume.weight = fadeIn ? progress : (1f - progress);
            }

            yield return null;
        }

        if (!fadeIn && blurVolume != null)
        {
            blurVolume.weight = 0f;
            blurVolume.gameObject.SetActive(false); 
        }
    }
}