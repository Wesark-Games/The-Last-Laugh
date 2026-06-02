using UnityEngine;
using System.Collections;

namespace Project.SaveSystem
{
    /// <summary>
    /// Анимация сохранения в углу экрана.
    /// Автоматически находит панели сохранения на текущей активной сцене.
    /// </summary>
    public class SaveAnimator : MonoBehaviour
    {
        [Header("[ НАСТРОЙКИ ТАЙМИНГОВ ]")]
        [SerializeField] private float fadeInDuration  = 0.3f;
        [Tooltip("Сколько секунд кролик будет прыгать из шляпы")]
        [SerializeField] private float holdDuration    = 2.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        private Coroutine animCoroutine;
        private CanvasGroup currentGroup;
        private GameObject currentSavePanel;
        private GameObject currentHatObject;

        /// <summary>
        /// Этот метод вызывается из SaveManager перед началом анимации, 
        /// чтобы принудительно найти UI сохранения на загруженной сцене.
        /// </summary>
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
                // ИГНОРИРОВАНИЕ ПАУЗЫ ДЛЯ АНИМАТОРА:
                // Заставляем сам компонент Animator обновляться независимо от Time.timeScale
                anim.updateMode = AnimatorUpdateMode.UnscaledTime;
                anim.Play(0, -1, 0f);
            }

            if (currentGroup != null)
                yield return StartCoroutine(FadeTo(1f, fadeInDuration));

            // ИСПРАВЛЕНИЕ: Ждем в реальном времени, даже если игра на паузе
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
                // ИСПРАВЛЕНИЕ: Используем unscaledDeltaTime вместо обычного deltaTime
                t += Time.unscaledDeltaTime / duration;
                currentGroup.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }

            currentGroup.alpha = target;
        }

        /// <summary>
        /// Принудительно останавливает анимацию и прячет шляпу.
        /// Вызывается при старте катсцен или открытии настроек.
        /// </summary>
        public void ForceHide()
        {
            if (animCoroutine != null)
            {
                StopCoroutine(animCoroutine);
                animCoroutine = null;
            }

            // Находим ссылки, если они ещё не были привязаны
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