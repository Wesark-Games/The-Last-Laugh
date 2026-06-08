using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Project.World
{
    public class ArrowHider : MonoBehaviour
    {
        [Header("[ СОХРАНЕНИЕ ]")]
        [SerializeField] private string saveID = "arrow_group_1";

        [System.Serializable]
        public struct ArrowStep
        {
            public GameObject element;
            public bool autoActivate;
            public UnityEvent onStepStart;
            public UnityEvent onStepComplete;
        }

        [Header("[ НАСТРОЙКА ]")]
        [SerializeField] private ArrowStep[] steps;
        [SerializeField] private bool hideOnTriggerEnter = true;
        [SerializeField] private bool fadeOut = true;
        [SerializeField] private float fadeDuration = 0.5f;

        private int currentIndex = 0;

        private void Start()
        {
            if (Project.SaveSystem.SaveManager.Instance != null)
            {
                currentIndex = Project.SaveSystem.SaveManager.Instance.Data.GetArrowProgress(saveID);
            }

            for (int i = 0; i < steps.Length; i++)
                if (steps[i].element != null) steps[i].element.SetActive(false);

            if (currentIndex < steps.Length && steps[currentIndex].autoActivate)
                ShowStep(currentIndex);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!hideOnTriggerEnter) return; 
            if (!other.CompareTag("Player")) return;
            if (IsCurrentStepActive()) AdvanceSequence();
        }

        public void AdvanceSequence()
        {
            if (steps == null || steps.Length == 0) return;
            if (currentIndex >= steps.Length) return;

            GameObject currentTarget = steps[currentIndex].element;
            if (currentTarget != null && currentTarget.activeSelf)
            {
                if (fadeOut) StartCoroutine(FadeOutArrow(currentTarget));
                else currentTarget.SetActive(false);
            }

            steps[currentIndex].onStepComplete?.Invoke();
            currentIndex++;

            if (Project.SaveSystem.SaveManager.Instance != null)
            {
                Project.SaveSystem.SaveManager.Instance.Data.SetArrowProgress(saveID, currentIndex);
            }

            if (currentIndex < steps.Length && steps[currentIndex].autoActivate)
                ShowStep(currentIndex);
        }


        public void ShowActiveElementFromCutscene()
        {
            if (currentIndex >= 0 && currentIndex < steps.Length)
            {
                ShowStep(currentIndex);
            }
        }

        public void ShowSpecificElement(int index)
        {
            if (index < 0 || index >= steps.Length) return;

            if (currentIndex >= 0 && currentIndex < steps.Length)
            {
                GameObject currentTarget = steps[currentIndex].element;
                if (currentTarget != null) currentTarget.SetActive(false);
            }

            currentIndex = index;
            ShowStep(currentIndex);
        }

    

        private void ShowStep(int index)
        {
            if (index < 0 || index >= steps.Length) return;
            GameObject target = steps[index].element;
            if (target != null) { ResetAlpha(target); target.SetActive(true); }
            steps[index].onStepStart?.Invoke();
        }

        private bool IsCurrentStepActive()
        {
            if (currentIndex >= 0 && currentIndex < steps.Length)
            {
                GameObject target = steps[currentIndex].element;
                return target != null && target.activeSelf;
            }
            return false;
        }

        private IEnumerator FadeOutArrow(GameObject arrow)
        {
            CanvasGroup cg = arrow.GetComponent<CanvasGroup>();
            SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                float alpha = Mathf.Lerp(1f, 0f, t);
                if (cg != null) cg.alpha = alpha;
                if (sr != null) { Color c = sr.color; c.a = alpha; sr.color = c; }
                yield return null;
            }
            arrow.SetActive(false);
        }

        private void ResetAlpha(GameObject arrow)
        {
            CanvasGroup cg = arrow.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
            SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
            if (sr != null) { Color c = sr.color; c.a = 1f; sr.color = c; }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawCube(transform.position, Vector3.one);
        }
    }
}