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

        // Шляпа
        public bool hasHat = false;

        // ВСЕ NPC НА СЦЕНЕ
        public List<NPCData> npcs = new List<NPCData>();
    }
}