using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Project.Movement;

namespace Project.SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        [Header("[ НАСТРОЙКИ ]")]
        [SerializeField] private string saveFileName = "savegame.json";
        [SerializeField] private bool   encryptSave  = false;
        [SerializeField] private string encryptKey   = "noir_clown_2024";

        public static SaveManager Instance { get; private set; }

        private SaveData    currentData;
        private string      savePath;
        private SaveAnimator saveAnimator;

        public SaveData Data => currentData;
        public bool IsLoadingSave { get; set; } = false;

        private void Awake()
{
    if (Instance != null && Instance != this)
    {
        Destroy(gameObject);
        return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    savePath = Path.Combine(Application.persistentDataPath, saveFileName);
    currentData = new SaveData(); // Оставляем чистым при старте
    saveAnimator = GetComponent<SaveAnimator>();

    Debug.Log($"[SaveManager] Путь к сохранению: {savePath}");
}

        public void StartNewGame(string gameSceneName)
        {
            IsLoadingSave = false;
            currentData = new SaveData(); 
            SceneManager.LoadScene(gameSceneName);
        }

        public void ContinueGame()
        {
            IsLoadingSave = true;
            LoadGame();
        }

        public void ReturnToGame()
        {
            IsLoadingSave = true; 
            if (!string.IsNullOrEmpty(currentData.currentScene))
            {
                SceneManager.LoadScene(currentData.currentScene);
            }
            else
            {
                SceneManager.LoadScene("Prologue"); 
            }
        }

        public void Save()
        {
            CollectCurrentState();
            WriteToFile();

            if (saveAnimator != null)
                saveAnimator.PlaySaveAnimation();

            Debug.Log($"[SaveManager] Сохранено мир. Чекпоинт: {currentData.checkpointIndex}");
        }

        public void SaveAtCheckpoint(int checkpointIndex)
        {
            currentData.checkpointIndex = checkpointIndex;
            Save();
        }

        public void UpdatePlayerPosition(Vector3 position)
        {
            if (currentData == null) currentData = new SaveData();
            currentData.playerX = position.x;
            currentData.playerY = position.y;
        }

        public bool Load()
        {
            if (!HasSave()) return false;

            try
            {
                string json = File.ReadAllText(savePath);

                if (encryptSave)
                    json = Decrypt(json);

                currentData = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveManager] Загружено. Сцена: {currentData.currentScene}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Ошибка загрузки: {e.Message}");
                currentData = new SaveData();
                return false;
            }
        }

        public void LoadGame()
        {
            if (!Load()) return;
            SceneManager.LoadScene(currentData.currentScene);
        }

        public void ApplyLoadedData()
        {
            if (currentData == null) return;

            // Восстанавливаем позицию игрока
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.position = new Vector2(currentData.playerX, currentData.playerY);
                    rb.linearVelocity = Vector2.zero;
                }
                else
                {
                    player.transform.position = new Vector3(currentData.playerX, currentData.playerY, 0f);
                }
            }

            // Восстанавливаем позиции и состояние NPC со сцены
            NPCMovement[] sceneNpcs = FindObjectsByType<NPCMovement>(FindObjectsSortMode.None);
            foreach (var npc in sceneNpcs)
            {
                NPCData savedNpc = currentData.npcs.Find(n => n.npcName == npc.gameObject.name);
                if (savedNpc != null)
                {
                    npc.transform.position = new Vector3(savedNpc.posX, savedNpc.posY, npc.transform.position.z);
                    npc.LoadState(savedNpc.movementType, savedNpc.routeCompleted);
                }
            }

            // Восстанавливаем активное задание
            if (!string.IsNullOrEmpty(currentData.activeQuestText))
            {
                QuestPanel questPanel = FindFirstObjectByType<QuestPanel>();
                if (questPanel != null)
                    questPanel.ShowQuest(currentData.activeQuestText);
            }
        }

        public void SetFlag(string flag)
        {
            if (!currentData.completedFlags.Contains(flag))
            {
                currentData.completedFlags.Add(flag);
                WriteToFile(); 
            }
        }

        public bool GetFlag(string flag)
        {
            if (currentData == null || currentData.completedFlags == null) return false;
            return currentData.completedFlags.Contains(flag);
        }

        public void SetActiveQuest(string questText, int questIndex = 0)
        {
            currentData.activeQuestText  = questText;
            currentData.activeQuestIndex = questIndex;
        }

        public bool HasSave() => File.Exists(savePath);

        public void DeleteSave()
        {
            if (File.Exists(savePath))
                File.Delete(savePath);

            currentData = new SaveData();
            Debug.Log("[SaveManager] Сохранение удалено.");
        }

        private void CollectCurrentState()
        {
            currentData.currentScene = SceneManager.GetActiveScene().name;
            currentData.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            // Запись игрока
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                currentData.playerX = player.transform.position.x;
                currentData.playerY = player.transform.position.y;
            }

            // Запись NPC
            currentData.npcs.Clear();
            NPCMovement[] sceneNpcs = FindObjectsByType<NPCMovement>(FindObjectsSortMode.None);
            foreach (var npc in sceneNpcs)
            {
                NPCData nData = new NPCData
                {
                    npcName = npc.gameObject.name,
                    posX = npc.transform.position.x,
                    posY = npc.transform.position.y,
                    movementType = npc.GetCurrentMovementType(),
                    routeCompleted = npc.IsRouteCompleted()
                };
                currentData.npcs.Add(nData);
            }
        }

        private void WriteToFile()
        {
            try
            {
                string json = JsonUtility.ToJson(currentData, prettyPrint: true);
                if (encryptSave) json = Encrypt(json);
                File.WriteAllText(savePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Ошибка записи: {e.Message}");
            }
        }

        private string Encrypt(string text)
        {
            char[] chars = text.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                chars[i] = (char)(chars[i] ^ encryptKey[i % encryptKey.Length]);
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(new string(chars)));
        }

        private string Decrypt(string encrypted)
        {
            byte[] bytes = Convert.FromBase64String(encrypted);
            char[] chars = System.Text.Encoding.UTF8.GetString(bytes).ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                chars[i] = (char)(chars[i] ^ encryptKey[i % encryptKey.Length]);
            return new string(chars);
        }

        [ContextMenu("Open Save Folder")]
public void OpenSavePath()
{
    // Это работает только в редакторе Unity
    System.Diagnostics.Process.Start(Application.persistentDataPath);
    Debug.Log($"[SaveManager] Папка открыта: {Application.persistentDataPath}");
}

[ContextMenu("Delete Save File")]
public void DeleteSaveFileFromEditor()
{
    if (File.Exists(savePath))
    {
        File.Delete(savePath);
        Debug.Log($"[SaveManager] Файл удален по пути: {savePath}");
    }
    else
    {
        Debug.LogWarning("[SaveManager] Файл для удаления не найден!");
    }
    
    // Сбрасываем данные в памяти, чтобы текущая сессия стала чистой
    currentData = new SaveData();
}
    }
}