using UnityEngine;
using UnityEditor;
using Project.Combat;

public class RangedEnemyCreator
{
    [MenuItem("Инструменты/Создать стреляющего врага")]
    public static void CreateRangedEnemy()
    {
        // ── Корень врага ──
        GameObject enemy = new GameObject("RangedEnemy");

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0) enemy.layer = enemyLayer;
        if (TagExists("Enemy")) enemy.tag = "Enemy";

        // Физика
        var rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        var col = enemy.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.6f, 1f);

        // Скрипты
        enemy.AddComponent<Health>();
        enemy.AddComponent<EnemyCore>();
        var det = enemy.AddComponent<DetectionModule>();
        var shoot = enemy.AddComponent<ShootingModule>();
        enemy.AddComponent<ItemDrop>();

        // ── Спрайт (ребёнок) ──
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(enemy.transform);
        visual.transform.localPosition = Vector3.zero;
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        Sprite spr = FindSprite("frame_000"); // спрайт по умолчанию
        if (spr != null) sr.sprite = spr;
        sr.color = new Color(1f, 0.8f, 0.8f); // лёгкий красноватый оттенок чтобы отличать

        // ── Назначить пулю в ShootingModule ──
        GameObject bullet = FindPrefab("Bullet");
        if (bullet != null)
        {
            var so = new SerializedObject(shoot);
            var prop = so.FindProperty("projectilePrefab");
            if (prop != null) prop.objectReferenceValue = bullet;
            so.ApplyModifiedProperties();
        }
        else
        {
            Debug.LogWarning("Префаб 'Bullet' не найден — назначь вручную в ShootingModule");
        }

        // ── Настроить слои в DetectionModule ──
        var sod = new SerializedObject(det);
        SetLayerMask(sod, "playerLayer", "Player");
        SetLayerMask(sod, "wallLayer", "Wall");
        // широкий обзор для стрелка
        var va = sod.FindProperty("viewAngle");
        if (va != null) va.floatValue = 120f;
        sod.ApplyModifiedProperties();

        // Поставить в центр вида
        var sv = SceneView.lastActiveSceneView;
        if (sv != null) enemy.transform.position = sv.pivot;

        Selection.activeGameObject = enemy;
        Undo.RegisterCreatedObjectUndo(enemy, "Create Ranged Enemy");
        Debug.Log("✅ Стреляющий враг создан! Назначь спрайт в Visual если нужно.");
    }

    static void SetLayerMask(SerializedObject so, string field, string layerName)
    {
        var prop = so.FindProperty(field);
        int layer = LayerMask.NameToLayer(layerName);
        if (prop != null && layer >= 0) prop.intValue = 1 << layer;
    }

    static Sprite FindSprite(string name)
    {
        var guids = AssetDatabase.FindAssets(name + " t:Sprite");
        if (guids.Length == 0) return null;
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static GameObject FindPrefab(string name)
    {
        var guids = AssetDatabase.FindAssets(name + " t:Prefab");
        if (guids.Length == 0) return null;
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static bool TagExists(string tag)
    {
        try { GameObject.FindWithTag(tag); return true; }
        catch { return false; }
    }
}