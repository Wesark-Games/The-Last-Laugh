using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Juggling
{
    public class EscapeSequence : MonoBehaviour
    {
        [Header("[ ОГОНЬ ]")]
        [SerializeField] private float fireStartY = -4f;
        [SerializeField] private float fireRiseSpeed = 0.5f;
        [SerializeField] private Color fireColor = new Color(1f, 0.4f, 0.1f, 0.7f);
        [SerializeField] private int fireParticleCount = 30;

        [Header("[ ТОЛПА ]")]
        [SerializeField] private GameObject crowdNpcPrefab;
        [SerializeField] private int crowdCount = 15;
        [SerializeField] private float crowdSpawnY = -6f;
        [SerializeField] private float crowdSpeed = 3f;
        [SerializeField] private float crowdSpawnWidth = 12f;

        [Header("[ ИГРОК ]")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float slowdownFactor = 0.3f;
        [SerializeField] private float pushForce = 2f;

        [Header("[ ЦЕЛЬ ]")]
        [Tooltip("Y координата до которой нужно добежать")]
        [SerializeField] private float escapeY = 10f;

        [Header("[ СОБЫТИЯ ]")]
        public UnityEvent OnEscapeStart;
        public UnityEvent OnEscapeComplete;
        public UnityEvent OnPlayerCaught;

        private bool _isRunning;
        private float _currentFireY;
        private List<GameObject> _fireParticles = new();
        private List<CrowdNPC> _crowdNpcs = new();
        private Rigidbody2D _playerRb;
        private bool _playerSlowed;

        public void StartEscape()
        {
            if (_isRunning) return;
            _isRunning = true;
            _currentFireY = fireStartY;

            if (playerTransform != null)
                _playerRb = playerTransform.GetComponentInParent<Rigidbody2D>();

            OnEscapeStart?.Invoke();
            StartCoroutine(SpawnFireEffect());
            StartCoroutine(SpawnCrowdWaves());
        }

        private void Update()
        {
            if (!_isRunning) return;

            // Огонь поднимается
            _currentFireY += fireRiseSpeed * Time.deltaTime;

            // Обновляем огненные частицы
            UpdateFireParticles();

            // Проверяем — игрок добежал до выхода?
            if (playerTransform != null && playerTransform.position.y >= escapeY)
            {
                _isRunning = false;
                OnEscapeComplete?.Invoke();
            }

            // Огонь догнал игрока?
            if (playerTransform != null && playerTransform.position.y <= _currentFireY + 1f)
            {
                _isRunning = false;
                OnPlayerCaught?.Invoke();
            }
        }

        private IEnumerator SpawnFireEffect()
        {
            for (int i = 0; i < fireParticleCount; i++)
            {
                GameObject fire = new GameObject("FireParticle");
                SpriteRenderer sr = fire.AddComponent<SpriteRenderer>();
                sr.color = fireColor;
                sr.sortingOrder = 50;

                // Создаём спрайт квадрат
                Texture2D tex = new Texture2D(8, 8);
                Color[] pixels = new Color[64];
                for (int p = 0; p < 64; p++)
                    pixels[p] = Color.white;
                tex.SetPixels(pixels);
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8);

                float randomX = Random.Range(-crowdSpawnWidth / 2, crowdSpawnWidth / 2);
                fire.transform.position = new Vector3(randomX, fireStartY + Random.Range(-1f, 1f), 0);
                fire.transform.localScale = new Vector3(
                    Random.Range(0.5f, 2f),
                    Random.Range(0.5f, 1.5f), 1);

                _fireParticles.Add(fire);
                yield return new WaitForSeconds(0.05f);
            }
        }

        private void UpdateFireParticles()
        {
            foreach (var fire in _fireParticles)
            {
                if (fire == null) continue;
                Vector3 pos = fire.transform.position;
                pos.y = _currentFireY + Random.Range(-0.5f, 1.5f);
                pos.x += Random.Range(-0.05f, 0.05f);
                fire.transform.position = pos;

                // Мерцание
                SpriteRenderer sr = fire.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    float flicker = Random.Range(0.5f, 1f);
                    sr.color = new Color(1f, Random.Range(0.2f, 0.6f), 0.1f, flicker * 0.8f);
                }
            }
        }

        private IEnumerator SpawnCrowdWaves()
        {
            // Волна 1 — сразу
            yield return new WaitForSeconds(0.5f);
            SpawnCrowdGroup(crowdCount / 3);

            // Волна 2
            yield return new WaitForSeconds(2f);
            SpawnCrowdGroup(crowdCount / 3);

            // Волна 3
            yield return new WaitForSeconds(2f);
            SpawnCrowdGroup(crowdCount / 3 + 1);
        }

        private void SpawnCrowdGroup(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float randomX = Random.Range(-crowdSpawnWidth / 2, crowdSpawnWidth / 2);
                Vector3 spawnPos = new Vector3(randomX, crowdSpawnY + Random.Range(-1f, 0f), 0);

                if (crowdNpcPrefab != null)
                {
                    GameObject npc = Instantiate(crowdNpcPrefab, spawnPos, Quaternion.identity);
                    CrowdNPC crowd = npc.GetComponent<CrowdNPC>();
                    if (crowd != null)
                    {
                        crowd.Init(crowdSpeed + Random.Range(-0.5f, 0.5f), playerTransform, slowdownFactor, pushForce);
                        _crowdNpcs.Add(crowd);
                    }
                }
                else
                {
                    // Без префаба — создаём простой NPC
                    SpawnSimpleCrowdNPC(spawnPos);
                }
            }
        }

        private void SpawnSimpleCrowdNPC(Vector3 pos)
        {
            GameObject npc = new GameObject("CrowdNPC");
            npc.transform.position = pos;
            npc.layer = LayerMask.NameToLayer("Default");

            SpriteRenderer sr = npc.AddComponent<SpriteRenderer>();
            // Тёмный силуэт
            sr.color = new Color(0.15f, 0.1f, 0.08f, 0.9f);
            sr.sortingOrder = 40;

            // Создаём спрайт
            Texture2D tex = new Texture2D(16, 24);
            Color[] pixels = new Color[16 * 24];
            for (int y = 0; y < 24; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    // Силуэт человека
                    bool isBody = (y < 16 && x >= 4 && x < 12);
                    bool isHead = (y >= 16 && y < 24 && x >= 5 && x < 11);
                    pixels[y * 16 + x] = (isBody || isHead) ? Color.white : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 16, 24), new Vector2(0.5f, 0.5f), 16);

            Rigidbody2D rb = npc.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.mass = 2f;

            BoxCollider2D col = npc.AddComponent<BoxCollider2D>();
            col.isTrigger = false;
            col.size = new Vector2(0.8f, 1.2f);

            CrowdNPC crowd = npc.AddComponent<CrowdNPC>();
            crowd.Init(crowdSpeed + Random.Range(-0.5f, 0.5f), playerTransform, slowdownFactor, pushForce);
            _crowdNpcs.Add(crowd);
        }

        private void OnDrawGizmos()
        {
            // Огонь
            Gizmos.color = new Color(1f, 0.3f, 0, 0.5f);
            Gizmos.DrawCube(new Vector3(0, fireStartY, 0), new Vector3(crowdSpawnWidth, 1, 0));

            // Толпа
            Gizmos.color = new Color(0.5f, 0, 0.5f, 0.3f);
            Gizmos.DrawCube(new Vector3(0, crowdSpawnY, 0), new Vector3(crowdSpawnWidth, 1, 0));

            // Выход
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(-10, escapeY, 0), new Vector3(10, escapeY, 0));
        }
    }
}
