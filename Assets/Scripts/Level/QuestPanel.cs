using UnityEngine;
using TMPro;
using System.Collections;

public class QuestPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TextMeshProUGUI questText;

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float hiddenX = -500f; // Координата за экраном
    [SerializeField] private float targetX = 20f;    // Координата на экране

    [Header("Timing Settings")]
    [Tooltip("Сколько секунд задание висит на экране перед тем как уехать обратно")]
    [SerializeField] private float displayDuration = 10f;

    private Coroutine animationRoutine;
    private Coroutine autoHideRoutine;

    private void Awake()
    {
        if (rectTransform == null) 
            rectTransform = GetComponent<RectTransform>();

        // На старте игры сразу прячем панель за левый край экрана
        Vector2 pos = rectTransform.anchoredPosition;
        pos.x = hiddenX;
        rectTransform.anchoredPosition = pos;
    }

    // ВАРИАНТ 1: Вызов без параметров
    public void ShowQuest()
    {
        Slide(targetX);
        StartAutoHide();
    }

    // ВАРИАНТ 2: Вызов с текстом
    public void ShowQuest(string newQuestMessage)
    {
        if (questText != null)
            questText.text = newQuestMessage;

        Slide(targetX);
        StartAutoHide();
    }

    // Метод для ручного скрытия панели, если нужно убрать её раньше времени
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
        // Если таймер автоскрытия уже шёл (например, игрок наступил на новый триггер), сбрасываем его
        if (autoHideRoutine != null)
            StopCoroutine(autoHideRoutine);

        autoHideRoutine = StartCoroutine(AutoHideSequence());
    }

    private IEnumerator AutoHideSequence()
    {
        // Ждем 10 секунд (или сколько указано в инспекторе)
        yield return new WaitForSeconds(displayDuration);
        
        // Плавно уводим панель обратно
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
            t = t * t * (3f - 2f * t); // SmoothStep сглаживание

            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
        animationRoutine = null;
    }
}