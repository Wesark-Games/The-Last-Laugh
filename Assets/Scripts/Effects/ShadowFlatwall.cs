using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
public class TilemapRotatedWallShadow : MonoBehaviour
{
    [Header("Настройки пиксельной тени")]
    public int shadowHeightInPixels = 4;
    public float pixelsPerUnit = 16f;
    [Range(0f, 1f)]
    public float shadowAlpha = 0.5f;

    void Start()
    {
        // 1. Получаем компоненты оригинальной стены
        Tilemap originalTilemap = GetComponent<Tilemap>();
        TilemapRenderer originalRenderer = GetComponent<TilemapRenderer>();

        // 2. Создаем объект под тень в той же позиции, БЕЗ смещения объекта
        GameObject shadowObj = new GameObject("Tilemap_RotatedShadow_Generated");
        shadowObj.transform.SetParent(transform);
        
        // Сдвигаем строго по Z чуть вперед к камере (-0.02f), чтобы не было мерцания с полом
        shadowObj.transform.localPosition = new Vector3(0f, 0f, -0.02f);
        shadowObj.transform.localRotation = Quaternion.identity;
        shadowObj.transform.localScale = Vector3.one;

        // 3. Добавляем компоненты тени
        Tilemap shadowTilemap = shadowObj.AddComponent<Tilemap>();
        TilemapRenderer shadowRenderer = shadowObj.AddComponent<TilemapRenderer>();

        // Настраиваем слои (строго под стеной)
        shadowRenderer.sortingLayerID = originalRenderer.sortingLayerID;
        shadowRenderer.sortingOrder = originalRenderer.sortingOrder - 1; 

        // Красим в полупрозрачный черный
        shadowRenderer.material = new Material(Shader.Find("Sprites/Default"));
        shadowRenderer.material.color = new Color(0f, 0f, 0f, shadowAlpha);

        // 4. КРИТИЧЕСКИЙ ШАГ: Попиксельно копируем тайлы и учитываем их индивидуальный поворот
        BoundsInt bounds = originalTilemap.cellBounds;

        // Включаем возможность изменять матрицу трансформации тайлов на объекте тени
        shadowTilemap.tileAnchor = originalTilemap.tileAnchor;
        shadowTilemap.orientation = originalTilemap.orientation;

        // Смещение в локальных координатах тайла (всегда вниз по его собственной оси Y)
        float localOffset = -(shadowHeightInPixels / pixelsPerUnit);
        Matrix4x4 shadowOffsetMatrix = Matrix4x4.Translate(new Vector3(0f, localOffset, 0f));

        // Проходим по всей сетке тайлмапа
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = originalTilemap.GetTile(pos);
            if (tile != null)
            {
                // Копируем тайл в ту же позицию на слое тени
                shadowTilemap.SetTile(pos, tile);

                // Получаем матрицу поворота/трансформации оригинального тайла
                Matrix4x4 originalMatrix = originalTilemap.GetTransformMatrix(pos);

                // Перемножаем матрицы: сначала применяем оригинальный поворот тайла, 
                // а затем смещаем его вниз относительно этого поворота
                Matrix4x4 finalMatrix = originalMatrix * shadowOffsetMatrix;

                // Устанавливаем итоговую матрицу для тайла тени
                shadowTilemap.SetTransformMatrix(pos, finalMatrix);
            }
        }
    }
}