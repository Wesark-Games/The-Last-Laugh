using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.SaveSystem
{
    [Serializable]
    public class NPCData
    {
        public string npcName;
        public float posX;
        public float posY;
        public string movementType;
        public bool routeCompleted;
    }

    [Serializable]
    public class SaveData
    {
        // Системные данные
        public string currentScene = "";
        public string lastSaveTime = "";
        public int checkpointIndex = 0;

        // Игрок
        public float playerX = 0f;
        public float playerY = 0f;

        // Квесты и флаги
        public string activeQuestText = "";
        public int activeQuestIndex = 0;
        public List<string> completedFlags = new List<string>();
        public int activeArrowIndex = 0;

        // Шляпа
        public bool hasHat = false;

        // NPC
        public List<NPCData> npcs = new List<NPCData>();

        // Данные для стрелок
        public List<string> arrowIDs = new List<string>();
        public List<int> arrowIndices = new List<int>();

        // Методы для работы с прогрессом стрелок
        public int GetArrowProgress(string id)
        {
            int index = arrowIDs.IndexOf(id);
            if (index != -1) return arrowIndices[index];
            return 0;
        }

        public void SetArrowProgress(string id, int index)
        {
            int foundIndex = arrowIDs.IndexOf(id);
            if (foundIndex != -1)
            {
                arrowIndices[foundIndex] = index;
            }
            else
            {
                arrowIDs.Add(id);
                arrowIndices.Add(index);
            }
        }
    }
}