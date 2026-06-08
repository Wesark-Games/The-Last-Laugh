using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimplePixelDownShadow : MonoBehaviour
{
    [Header("Настройки пиксельной тени")]
    public int shadowOffsetPixels = 4;
    public float pixelsPerUnit = 32f;

    [Range(0f, 1f)]
    public float shadowColorAlpha = 0.5f;

    private SpriteRenderer mainRenderer;
    private SpriteRenderer shadowRenderer;

    void Start()
    {
        mainRenderer = GetComponent<SpriteRenderer>();

        // Создаем дочерний объект для тени
        GameObject shadowObj = new GameObject(gameObject.name + "_Shadow");
        shadowObj.transform.SetParent(transform);

        // Сдвигаем строго вниз по Y на нужные пиксели
        float yOffset = -(shadowOffsetPixels / pixelsPerUnit);
        
        // Сдвиг по Z 
        shadowObj.transform.localPosition = new Vector3(0f, yOffset, -0.01f);
        shadowObj.transform.localRotation = Quaternion.identity;
        shadowObj.transform.localScale = Vector3.one;

        shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
        shadowRenderer.sprite = mainRenderer.sprite;
        shadowRenderer.sortingLayerID = mainRenderer.sortingLayerID;
        
        // Отрисовка на один порядок ниже родителя, чтобы тень не перекрывала сам предмет
        shadowRenderer.sortingOrder = mainRenderer.sortingOrder - 1;

        shadowRenderer.material = new Material(Shader.Find("Sprites/Default"));
        shadowRenderer.material.color = new Color(0f, 0f, 0f, shadowColorAlpha);
    }


    void LateUpdate()
    {
        if (mainRenderer == null || shadowRenderer == null) return;

        shadowRenderer.sprite = mainRenderer.sprite;
        shadowRenderer.flipX = mainRenderer.flipX;
        shadowRenderer.flipY = mainRenderer.flipY;
        shadowRenderer.enabled = mainRenderer.enabled;
    }
}