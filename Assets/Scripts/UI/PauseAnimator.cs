using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{

    [RequireComponent(typeof(CanvasGroup))]
    public class PauseAnimator : MonoBehaviour
    {
   
        [Header("[ АНИМАЦИЯ ]")]
        [SerializeField] private float showDuration = 0.25f;
        [SerializeField] private float hideDuration = 0.2f;
        [SerializeField] private float startScale = 0.9f;
    

        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Coroutine currentCoroutine;

        private void Awake()
        {
            canvasGroup   = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
        }

     
        public void Show()
        {
            gameObject.SetActive(true);

            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(ShowRoutine());
        }

        public void Hide(System.Action onComplete = null)
        {
            if (currentCoroutine != null)
                StopCoroutine(currentCoroutine);

            currentCoroutine = StartCoroutine(HideRoutine(onComplete));
        }

        private IEnumerator ShowRoutine()
        {
            canvasGroup.alpha          = 0f;
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;
            rectTransform.localScale   = Vector3.one * startScale;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / showDuration;
                float eased = EaseOutCubic(t);

                canvasGroup.alpha        = Mathf.Lerp(0f, 1f, eased);
                rectTransform.localScale = Vector3.Lerp(
                    Vector3.one * startScale,
                    Vector3.one,
                    eased
                );

                yield return null;
            }

            canvasGroup.alpha          = 1f;
            canvasGroup.interactable   = true;
            canvasGroup.blocksRaycasts = true;
            rectTransform.localScale   = Vector3.one;
        }

        private IEnumerator HideRoutine(System.Action onComplete)
        {
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = false;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / hideDuration;
                float eased = EaseInCubic(t);

                canvasGroup.alpha        = Mathf.Lerp(1f, 0f, eased);
                rectTransform.localScale = Vector3.Lerp(
                    Vector3.one,
                    Vector3.one * startScale,
                    eased
                );

                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }


        private float EaseOutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        private float EaseInCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * t;
        }
    }
}