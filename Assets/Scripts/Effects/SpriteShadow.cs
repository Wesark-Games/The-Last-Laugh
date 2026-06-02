using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimplePixelDownShadow : MonoBehaviour
{
    [Header("Настройки пиксельной тени")]
    [Tooltip("Смещение тени вниз в пикселях (например, 3, 4, 5)")]
    public int shadowOffsetPixels = 4;

    [Tooltip("Количество пикселей в одном юните (PPU вашего спрайта, обычно 16 или 32)")]
    public float pixelsPerUnit = 32f;

    [Tooltip("Прозрачность тени (от 0 до 1)")]
    [Range(0f, 1f)]
    public float shadowColorAlpha = 0.5f;

    private SpriteRenderer mainRenderer;
    private SpriteRenderer shadowRenderer;

    void Start()
    {
        mainRenderer = GetComponent<SpriteRenderer>();

        // 1. Создаем дочерний объект для тени
        GameObject shadowObj = new GameObject(gameObject.name + "_Shadow");
        shadowObj.transform.SetParent(transform);

        // 2. Сдвигаем строго вниз по Y на нужные пиксели
        float yOffset = -(shadowOffsetPixels / pixelsPerUnit);
        
        // Сдвиг по Z (-0.01f), чтобы в вашей 3D-сцене URP тень ложилась ПРАВИЛЬНО (чуть ближе к камере, чем пол)
        shadowObj.transform.localPosition = new Vector3(0f, yOffset, -0.01f);
        shadowObj.transform.localRotation = Quaternion.identity;
        shadowObj.transform.localScale = Vector3.one;

        // 3. Добавляем и настраиваем SpriteRenderer тени
        shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = mainRenderer.sprite;
        shadowRenderer.sortingLayerID = mainRenderer.sortingLayerID;
        
        // Отрисовка на один порядок ниже родителя, чтобы тень не перекрывала сам предмет
        shadowRenderer.sortingOrder = mainRenderer.sortingOrder - 1;

        // 4. Красим в полупрозрачный черный
        shadowRenderer.material = new Material(Shader.Find("Sprites/Default"));
        shadowRenderer.material.color = new Color(0f, 0f, 0f, shadowColorAlpha);
    }

    // Если предмет динамический (например, его можно двигать/пинать) или у него меняется спрайт
    void LateUpdate()
    {
        if (mainRenderer == null || shadowRenderer == null) return;

        // Синхронизируем кадры, если у предмета есть анимация или его перевернули (Flip)
        shadowRenderer.sprite = mainRenderer.sprite;
        shadowRenderer.flipX = mainRenderer.flipX;
        shadowRenderer.flipY = mainRenderer.flipY;
        shadowRenderer.enabled = mainRenderer.enabled;
    }
}