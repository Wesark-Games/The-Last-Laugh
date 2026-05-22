using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOmniShadow : MonoBehaviour
{
    [Header("Настройки круговой тени")]
    [Tooltip("Размер тени во все стороны в пикселях (например, 3, 4, 5)")]
    public int shadowSizeInPixels = 4;

    [Tooltip("Количество пикселей в одном юните (PPU вашего спрайта)")]
    public float pixelsPerUnit = 16f;

    [Tooltip("Прозрачность тени (от 0 до 1)")]
    [Range(0f, 1f)]
    public float shadowAlpha = 0.3f;

    private SpriteRenderer mainRenderer;
    private SpriteRenderer[] shadowRenderers = new SpriteRenderer[4];

    void Start()
    {
        mainRenderer = GetComponent<SpriteRenderer>();

        // Высчитываем сдвиг в координатах Unity
        float offset = shadowSizeInPixels / pixelsPerUnit;

        // Направления сдвига: Влево, Вправо, Вниз, Вверх
        // Сдвиг по Z (0.01f * i) нужен, чтобы слои тени не мерцали между собой
        Vector3[] directions = new Vector3[]
        {
            new Vector3(-offset, 0f, 0.01f),
            new Vector3(offset, 0f, 0.02f),
            new Vector3(0f, -offset, 0.03f),
            new Vector3(0f, offset, 0.04f)
        };

        // Создаем 4 слоя тени
        for (int i = 0; i < 4; i++)
        {
            GameObject shadowObj = new GameObject($"SpriteShadow_Dir_{i}");
            shadowObj.transform.SetParent(transform);
            
            // Устанавливаем локальное смещение
            shadowObj.transform.localPosition = directions[i];
            shadowObj.transform.localRotation = Quaternion.identity;
            shadowObj.transform.localScale = Vector3.one;

            // Добавляем рендерер тени
            SpriteRenderer sRenderer = shadowObj.AddComponent<SpriteRenderer>();
            shadowRenderers[i] = sRenderer;

            // Настраиваем слои отрисовки, чтобы тень была строго под объектом
            sRenderer.sortingLayerID = mainRenderer.sortingLayerID;
            sRenderer.sortingOrder = mainRenderer.sortingOrder - 1;

            // Создаем уникальный материал и красим его в черный цвет
            sRenderer.material = new Material(Shader.Find("Sprites/Default"));
            sRenderer.material.color = new Color(0f, 0f, 0f, shadowAlpha);
        }
    }

    // Используем LateUpdate, чтобы тени обновлялись ПОСЛЕ того, 
    // как отработают все анимации или перемещения объекта в текущем кадре
    void LateUpdate()
    {
        if (mainRenderer == null) return;

        // Пробегаемся по всем 4 слоям тени и синхронизируем их состояние
        for (int i = 0; i < shadowRenderers.Length; i++)
        {
            if (shadowRenderers[i] != null)
            {
                // Синхронизируем текущий кадр анимации
                shadowRenderers[i].sprite = mainRenderer.sprite;
                
                // Синхронизируем отражение по горизонтали/вертикали (Flip)
                shadowRenderers[i].flipX = mainRenderer.flipX;
                shadowRenderers[i].flipY = mainRenderer.flipY;
                
                // Если оригинальный объект внезапно отключают — отключаем и тени
                shadowRenderers[i].enabled = mainRenderer.enabled;
            }
        }
    }
}