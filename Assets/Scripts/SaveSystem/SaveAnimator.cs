using UnityEngine;
using System.Collections;

namespace Project.SaveSystem
{
    public class SaveAnimator : MonoBehaviour
    {
        [Header("[ НАСТРОЙКИ ТАЙМИНГОВ ]")]
        [SerializeField] private float fadeInDuration  = 0.3f;
        [SerializeField] private float holdDuration    = 2.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        private Coroutine animCoroutine;
        private CanvasGroup currentGroup;
        private GameObject currentSavePanel;
        private GameObject currentHatObject;

    
        public void FindSceneUIReferences()
        {
            // Находим абсолютно все CanvasGroup на сцене, даже выключенные
            CanvasGroup[] allGroups = Resources.FindObjectsOfTypeAll<CanvasGroup>();

            foreach (CanvasGroup group in allGroups)
            {
                // Проверяем, что объект принадлежит сцене, а не является префабом в папке Project
                if (group.gameObject.scene.name == null) continue;

                if (group.gameObject.name == "SavePanel" || group.gameObject.CompareTag("SaveUI"))
                {
                    currentSavePanel = group.gameObject;
                    currentGroup = group;

                    Transform hatTransform = currentSavePanel.transform.Find("HatObject");
                    if (hatTransform != null)
                    {
                        currentHatObject = hatTransform.gameObject;
                    }
                    break;
                }
            }
        }

        public void PlaySaveAnimation()
        {
            FindSceneUIReferences();

            if (currentSavePanel == null || currentHatObject == null)
            {
                Debug.LogWarning("[SaveAnimator] UI сохранения (SavePanel или HatObject) не найден на текущей сцене!");
                return;
            }

            if (animCoroutine != null)
                StopCoroutine(animCoroutine);

            animCoroutine = StartCoroutine(SaveAnimationRoutine());
        }

        private IEnumerator SaveAnimationRoutine()
        {
            currentSavePanel.SetActive(true);
            currentHatObject.SetActive(true);

            Animator anim = currentHatObject.GetComponent<Animator>();
            if (anim != null)
            {
        
                // Заставляем сам компонент Animator обновляться независимо от Time.timeScale
                anim.updateMode = AnimatorUpdateMode.UnscaledTime;
                anim.Play(0, -1, 0f);
            }

            if (currentGroup != null)
                yield return StartCoroutine(FadeTo(1f, fadeInDuration));

        
            yield return new WaitForSecondsRealtime(holdDuration);

            if (currentGroup != null)
                yield return StartCoroutine(FadeTo(0f, fadeOutDuration));

            currentSavePanel.SetActive(false);
            currentHatObject.SetActive(false);
            
            animCoroutine = null;
        }

        private IEnumerator FadeTo(float target, float duration)
        {
            if (currentGroup == null) yield break;

            float start = currentGroup.alpha;
            float t     = 0f;

            while (t < 1f)
            {
        
                t += Time.unscaledDeltaTime / duration;
                currentGroup.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }

            currentGroup.alpha = target;
        }

      
        public void ForceHide()
        {
            if (animCoroutine != null)
            {
                StopCoroutine(animCoroutine);
                animCoroutine = null;
            }

           
            if (currentSavePanel == null || currentHatObject == null)
            {
                FindSceneUIReferences();
            }

            // Мгновенно гасим UI
            if (currentSavePanel != null) currentSavePanel.SetActive(false);
            if (currentHatObject != null) currentHatObject.SetActive(false);
            if (currentGroup != null)     currentGroup.alpha = 0f;
        }
    }
}