using UnityEngine;
using TMPro;
using System.Collections;

public class FadeHintWithPanel : MonoBehaviour
{
    [Header("UI Components")]
    public CanvasGroup hintGroup; 
    public TextMeshProUGUI hintText; 

    [Header("Hint Settings")]
    [TextArea(3, 5)]
    public string message = "Заперто";
    public float fadeDuration = 0.3f; 

    private Coroutine activeFade; 

    private void Start()
    {
        if (hintGroup != null)
        {
            hintGroup.alpha = 0f;
            hintGroup.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (hintGroup != null && hintText != null)
            {
                hintText.text = message;
                StartFade(1f); 
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (hintGroup != null)
            {
                StartFade(0f); 
            }
        }
    }

    private void StartFade(float targetAlpha)
    {
        if (activeFade != null)
        {
            StopCoroutine(activeFade);
        }
        activeFade = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (targetAlpha > 0)
        {
            hintGroup.gameObject.SetActive(true);
        }

        float startAlpha = hintGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            hintGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null; 
        }

        hintGroup.alpha = targetAlpha; 

        if (targetAlpha <= 0)
        {
            hintGroup.gameObject.SetActive(false);
        }

        activeFade = null; 
    }
}