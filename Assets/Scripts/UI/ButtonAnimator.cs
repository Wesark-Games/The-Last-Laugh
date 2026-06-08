using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Project.UI
{

    [RequireComponent(typeof(Image))]
    public class NoirButtonAnimator : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
       
        [Header("[ НАВЕДЕНИЕ ]")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float animationSpeed = 8f;

        [Header("[ НАЖАТИЕ ]")]
        [SerializeField] private Color pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private float pressedScale = 0.96f;

        [Header("[ ЗВУК ]")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private AudioSource audioSource;


        private Vector3 originalScale;
        private Vector3 targetScale;
        private Image image;
        private Color originalColor;
        private bool isPressed;

        private void Awake()
        {
            image         = GetComponent<Image>();
            originalScale = transform.localScale;
            targetScale   = originalScale;
            originalColor = image.color;
        }

        private void Update()
        {
            // Плавная анимация масштаба
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.unscaledDeltaTime * animationSpeed
            );
        }



        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isPressed)
                targetScale = originalScale * hoverScale;

            PlaySound(hoverSound);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isPressed)
                targetScale = originalScale;

            image.color = originalColor;
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed       = true;
            targetScale     = originalScale * pressedScale;
            image.color     = pressedColor;

            PlaySound(clickSound);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed   = false;
            targetScale = originalScale * hoverScale;
            image.color = originalColor;
        }



        private void PlaySound(AudioClip clip)
        {
            if (audioSource == null || clip == null) return;
            audioSource.PlayOneShot(clip);
        }
    }
}