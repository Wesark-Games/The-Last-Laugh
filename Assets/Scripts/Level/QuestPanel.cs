using UnityEngine;
using TMPro;
using System.Collections;
using Project.SaveSystem;

public class QuestPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TextMeshProUGUI questText;

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float hiddenX = -500f; 
    [SerializeField] private float targetX = 20f;    

    [Header("Timing Settings")]
    [SerializeField] private float displayDuration = 10f;

    private Coroutine animationRoutine;
    private Coroutine autoHideRoutine;

    private void Awake()
    {
        if (rectTransform == null) 
            rectTransform = GetComponent<RectTransform>();

        Vector2 pos = rectTransform.anchoredPosition;
        pos.x = hiddenX;
        rectTransform.anchoredPosition = pos;
    }

    private void Start()
    {
        if (SaveManager.Instance != null && !string.IsNullOrEmpty(SaveManager.Instance.Data.activeQuestText))
        {
            if (questText != null)
            {
                questText.text = SaveManager.Instance.Data.activeQuestText;
            }
        }
    }

    // Вызов без параметров
    public void ShowQuest()
    {
        Slide(targetX);
        StartAutoHide();
    }

    // Вызов с текстом
    public void ShowQuest(string newQuestMessage)
    {
        if (questText != null)
            questText.text = newQuestMessage;

        // Сохраняем активное задание в менеджер сохранений
        if (SaveManager.Instance != null)
        {
            // Здесь напрямую пишем в Data
            SaveManager.Instance.Data.activeQuestText = newQuestMessage;
        }

        Slide(targetX);
        StartAutoHide();
    }

    // Метод для ручного скрытия панели
    public void HideQuest()
    {
        if (autoHideRoutine != null)
        {
            StopCoroutine(autoHideRoutine);
            autoHideRoutine = null;
        }
        Slide(hiddenX);
    }

    private void StartAutoHide()
    {
        if (autoHideRoutine != null)
            StopCoroutine(autoHideRoutine);

        autoHideRoutine = StartCoroutine(AutoHideSequence());
    }

    private IEnumerator AutoHideSequence()
    {
        yield return new WaitForSeconds(displayDuration);
        Slide(hiddenX);
        autoHideRoutine = null;
    }

    private void Slide(float targetXPosition)
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(SlideRoutine(targetXPosition));
    }

    private IEnumerator SlideRoutine(float targetValue)
    {
        float time = 0;
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 targetPosition = new Vector2(targetValue, startPosition.y);

        while (time < slideDuration)
        {
            time += Time.deltaTime;
            
            float t = time / slideDuration;
            t = t * t * (3f - 2f * t); // SmoothStep

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        animationRoutine = null;
    }
}