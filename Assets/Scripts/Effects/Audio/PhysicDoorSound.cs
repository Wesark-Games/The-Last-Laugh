using UnityEngine;

namespace Project.Environment
{
    [RequireComponent(typeof(AudioSource))]
    public class PhysicalDoorAudioDistance : MonoBehaviour
    {
        [Header("[ ЗВУКИ ]")]
        [SerializeField] private AudioClip[] doorOpenSounds;
        [Range(0f, 1f)] [SerializeField] private float volume = 0.7f;

        [Header("[ ДИСТАНЦИЯ ]")]
        [Tooltip("На каком расстоянии срабатывает звук при приближении")]
        [SerializeField] private float triggerDistance = 1.2f;
        
        [Tooltip("Как далеко нужно отойти, чтобы звук перезарядился")]
        [SerializeField] private float resetDistance = 3.5f;

        private AudioSource audioSource;
        private Transform playerTransform;
        private bool isReadyToPlay = true;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;

            // Автоматически находим игрока на сцене по тегу
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (playerTransform == null) return;

            // Считаем точное расстояние между игроком и дверью
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            // 1. УДАР: Если подошел вплотную и звук готов к воспроизведению
            if (distance <= triggerDistance && isReadyToPlay)
            {
                PlayRandomOpenSound();
                isReadyToPlay = false; // Блокируем новые звуки
            }

            // 2. ПЕРЕЗАРЯДКА: Звук разблокируется, только если отойти подальше
            if (distance >= resetDistance && !isReadyToPlay)
            {
                isReadyToPlay = true; 
            }
        }

        private void PlayRandomOpenSound()
        {
            if (doorOpenSounds == null || doorOpenSounds.Length == 0) return;

            int randomIndex = Random.Range(0, doorOpenSounds.Length);
            AudioClip selectedClip = doorOpenSounds[randomIndex];

            if (selectedClip != null)
            {
                audioSource.clip = selectedClip;
                audioSource.volume = volume;
                audioSource.Play();
            }
        }

        // Визуализация радиусов в редакторе Unity (красный — удар, зеленый — отойти)
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, resetDistance);
        }
    }
}