using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using Project.Combat;

public class WalkingShooterCreator
{
    [MenuItem("Инструменты/Создать ходячего стрелка")]
    public static void Create()
    {
        GameObject enemy = new GameObject("WalkingShooter");

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0) enemy.layer = enemyLayer;
        if (TagExists("Enemy")) enemy.tag = "Enemy";

        // Физика — кинематик (двигает NavMeshAgent, но триггеры пуль работают)
        var rb = enemy.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        var col = enemy.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.6f, 1f);

        // NavMeshAgent (настройка под 2D)
        var agent = enemy.AddComponent<NavMeshAgent>();
        agent.radius = 0.3f;
        agent.height = 1f;
        agent.speed = 3f;
        agent.acceleration = 20f;
        agent.angularSpeed = 0f;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.autoBraking = true;

        // Скрипты
        enemy.AddComponent<Health>();
        enemy.AddComponent<EnemyCore>();
        var det = enemy.AddComponent<DetectionModule>();
        enemy.AddComponent<ShootingModule>();
        enemy.AddComponent<EnemyNavMovement>();
        enemy.AddComponent<ItemDrop>();

        // Спрайт-ребёнок
        GameObject visual = new GameObject("Visual");
        visual.transform.SetParent(enemy.transform);
        visual.transform.localPosition = Vector3.zero;
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        sr.color = new Color(1f, 0.85f, 0.85f);
        Sprite spr = FindSprite("frame_000");
        if (spr != null) sr.sprite = spr;

        // Назначить пулю
        GameObject bullet = FindPrefab("Bullet");
        if (bullet != null)
        {
            foreach (var sm in enemy.GetComponents<ShootingModule>())
            {
                var so = new SerializedObject(sm);
                var p = so.FindProperty("projectilePrefab");
                if (p != null) { p.objectReferenceValue = bullet; so.ApplyModifiedProperties(); }
            }
        }

        // Слои в DetectionModule
        var sod = new SerializedObject(det);
        SetMask(sod, "playerLayer", "Player");
        SetMask(sod, "wallLayer", "Wall");
        sod.ApplyModifiedProperties();

        var sv = SceneView.lastActiveSceneView;
        if (sv != null) enemy.transform.position = sv.pivot;

        Selection.activeGameObject = enemy;
        Undo.RegisterCreatedObjectUndo(enemy, "Create Walking Shooter");
        Debug.Log("✅ Ходячий стрелок создан! Не забудь запечь NavMesh (Bake).");
    }

    static void SetMask(SerializedObject so, string field, string layer)
    {
        var p = so.FindProperty(field);
        int l = LayerMask.NameToLayer(layer);
        if (p != null && l >= 0) p.intValue = 1 << l;
    }

    static Sprite FindSprite(string name)
    {
        var g = AssetDatabase.FindAssets(name + " t:Sprite");
        if (g.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g[0]));
    }

    static GameObject FindPrefab(string name)
    {
        var g = AssetDatabase.FindAssets(name + " t:Prefab");
        if (g.Length == 0) return null;
        return AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g[0]));
    }

    static bool TagExists(string tag)
    {
        try { GameObject.FindWithTag(tag); return true; }
        catch { return false; }
    }
}