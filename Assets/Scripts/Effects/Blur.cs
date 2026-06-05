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

    // ВЫЗЫВАТЬ В НАЧАЛЕ КАТСЦЕНЫ
    public void BlurIn()
    {
        // КРИТИЧЕСКОЕ ИСПРАВЛЕНИЕ: Включаем объект ДО запуска корутины
        if (blurVolume != null)
        {
            blurVolume.gameObject.SetActive(true);
        }

        if (blurCoroutine != null) StopCoroutine(blurCoroutine);
        blurCoroutine = StartCoroutine(FadeBlurRoutine(true));
    }

    // ВЫЗЫВАТЬ В КОНЦЕ КАТСЦЕНЫ
    public void BlurOut()
    {
        // Здесь объект уже активен, так что корутина стартует без проблем
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

        // Если это был выход из размытия — полностью тушим объект
        if (!fadeIn && blurVolume != null)
        {
            blurVolume.weight = 0f;
            blurVolume.gameObject.SetActive(false); 
        }
    }
}