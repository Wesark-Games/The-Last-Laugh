using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
public class TilemapWallShadow : MonoBehaviour
{
    [Header("Настройки пиксельной тени")]
    public int shadowHeightInPixels = 4;
    public float pixelsPerUnit = 16f;
    [Range(0f, 1f)]
    public float shadowAlpha = 0.5f;

    void Start()
    {
        // 1. Получаем оригинальные компоненты стены
        Tilemap originalTilemap = GetComponent<Tilemap>();
        TilemapRenderer originalRenderer = GetComponent<TilemapRenderer>();

        // 2. Создаем объект под тень
        GameObject shadowObj = new GameObject("Tilemap_Shadow_Generated");
        shadowObj.transform.SetParent(transform);
        
        float pixelOffset = -(shadowHeightInPixels / pixelsPerUnit);
        // Сдвигаем по Z чуть ВПЕРЕД (в 2D это уменьшение Z, например -0.05f), 
        // чтобы тень была ближе к камере, чем пол, но за счет Sorting Order оставалась под стеной
        shadowObj.transform.localPosition = new Vector3(0f, pixelOffset, -0.05f);
        shadowObj.transform.localRotation = Quaternion.identity;
        shadowObj.transform.localScale = Vector3.one;

        // 3. Добавляем компоненты тени
        Tilemap shadowTilemap = shadowObj.AddComponent<Tilemap>();
        TilemapRenderer shadowRenderer = shadowObj.AddComponent<TilemapRenderer>();

        // 4. КРИТИЧЕСКИЙ ШАГ: Копируем данные тайлов из стены в тень
        var bounds = originalTilemap.cellBounds;
        TileBase[] allTiles = originalTilemap.GetTilesBlock(bounds);
        shadowTilemap.SetTilesBlock(bounds, allTiles);

        // 5. Настраиваем слои отрисовки
        shadowRenderer.sortingLayerID = originalRenderer.sortingLayerID;
        shadowRenderer.sortingOrder = originalRenderer.sortingOrder - 1; 

        // 6. Красим в черный цвет
        shadowRenderer.material = new Material(Shader.Find("Sprites/Default"));
        shadowRenderer.material.color = new Color(0f, 0f, 0f, shadowAlpha);
    }
}