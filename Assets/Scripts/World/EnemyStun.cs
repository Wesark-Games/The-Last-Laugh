using System.Collections;
using UnityEngine;

namespace Project.World
{
    /// <summary>
    /// Компонент оглушения врага от удара дверью.
    /// Добавляй на врагов.
    /// </summary>
    public class EnemyStun : MonoBehaviour
    {
        [Header("[ ВИЗУАЛ ОГЛУШЕНИЯ ]")]
        [Tooltip("Объект который показывается над врагом когда оглушён")]
        [SerializeField] private GameObject stunVisual;

        public bool IsStunned { get; private set; }

        private Coroutine stunCoroutine;

        public void Stun(float duration)
        {
            if (stunCoroutine != null)
                StopCoroutine(stunCoroutine);

            stunCoroutine = StartCoroutine(StunRoutine(duration));
        }

        private IEnumerator StunRoutine(float duration)
        {
            IsStunned = true;

            if (stunVisual != null)
                stunVisual.SetActive(true);

            // TODO: отключить AI врага когда будет готов EnemyCore
            // MonoBehaviour ai = GetComponent<Project.Core.EnemyCore>();
            // if (ai != null) ai.enabled = false;

            yield return new WaitForSeconds(duration);

            IsStunned = false;

            if (stunVisual != null)
                stunVisual.SetActive(false);

            // TODO: включить AI обратно
            // if (ai != null) ai.enabled = true;
        }
    }
}