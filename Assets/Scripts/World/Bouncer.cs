using UnityEngine;
using TMPro;
using System.Collections;

public class AutopilotBarrier : MonoBehaviour
{
    [Header("[ UI ]")]
    public CanvasGroup       hintGroup;
    public TextMeshProUGUI   hintText;

    [Header("[ ТЕКСТ ]")]
    [TextArea(3, 5)]
    public string message          = "Туда нельзя.";
    public float  fadeDuration     = 0.3f;
    [Tooltip("Минимальное время показа текста в секундах")]
    public float  minShowDuration  = 2f;

    [Header("[ АВТОПИЛОТ ]")]
    public float autoMoveSpeed    = 3f;
    public float autoMoveDuration = 0.5f;

    private Coroutine autopilotRoutine;
    private Coroutine fadeRoutine;
    private Coroutine hideRoutine;

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
        if (!collision.CompareTag("Player")) return;

        Project.Player.PlayerController player = collision.GetComponent<Project.Player.PlayerController>();
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

        if (player != null && rb != null && autopilotRoutine == null)
        {
            // Направление отката — противоположно движению
            Vector2 rollback = -rb.linearVelocity.normalized;

            if (rollback.sqrMagnitude < 0.01f)
                rollback = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;

            autopilotRoutine = StartCoroutine(Autopilot(player, rb, rollback));
        }

        // Показываем текст и отменяем таймер скрытия если он был
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        ShowText();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        // Скрываем текст не сразу а через minShowDuration
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    // ─── АВТОПИЛОТ ────────────────────────────────────────────────────────

    private IEnumerator Autopilot(Project.Player.PlayerController player, Rigidbody2D rb, Vector2 direction)
    {
        // Отключаем управление
        player.SetMovementEnabled(false);

        // Устанавливаем направление взгляда в сторону отката
        Vector2 cardinalDir = GetCardinalDirection(direction);
        player.SetFacingDirection(cardinalDir);

        // Принудительно включаем анимацию ходьбы через Animator
        Animator anim = player.GetComponent<Animator>();
        if (anim != null)
            anim.SetBool("IsMoving", true);

        float timer = 0f;

        while (timer < autoMoveDuration)
        {
            // Двигаем физикой
            rb.linearVelocity = direction * autoMoveSpeed;

            // Обновляем анимацию каждый кадр
            if (anim != null)
            {
                anim.SetFloat("DirX", Mathf.Abs(cardinalDir.x) > 0.1f ? -Mathf.Abs(cardinalDir.x) : cardinalDir.x);
                anim.SetFloat("DirY", cardinalDir.y);
                anim.SetBool("IsMoving", true);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Останавливаем
        rb.linearVelocity = Vector2.zero;

        if (anim != null)
            anim.SetBool("IsMoving", false);

        // Возвращаем управление
        player.SetMovementEnabled(true);

        autopilotRoutine = null;
    }

    // ─── ТЕКСТ ────────────────────────────────────────────────────────────

    private void ShowText()
    {
        if (hintGroup == null || hintText == null) return;

        hintText.text = message;
        hintGroup.gameObject.SetActive(true);

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(1f));
    }

    private IEnumerator HideAfterDelay()
    {
        // Ждём минимальное время показа
        yield return new WaitForSeconds(minShowDuration);

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(0f, deactivateOnEnd: true));

        hideRoutine = null;
    }

    private IEnumerator FadeTo(float target, bool deactivateOnEnd = false)
    {
        if (hintGroup == null) yield break;

        float start = hintGroup.alpha;
        float t     = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            hintGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        hintGroup.alpha = target;

        if (deactivateOnEnd && target <= 0f)
            hintGroup.gameObject.SetActive(false);

        fadeRoutine = null;
    }

    // ─── УТИЛИТЫ ──────────────────────────────────────────────────────────

    private Vector2 GetCardinalDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if      (angle >= -45f  && angle < 45f)   return Vector2.right;
        else if (angle >= 45f   && angle < 135f)  return Vector2.up;
        else if (angle >= -135f && angle < -45f)  return Vector2.down;
        else                                       return Vector2.left;
    }
}