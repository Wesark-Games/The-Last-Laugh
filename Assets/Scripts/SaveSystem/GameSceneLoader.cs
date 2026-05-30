using UnityEngine;
using System.Collections;

namespace Project.SaveSystem
{
    public class GameSceneLoader : MonoBehaviour
    {
        private IEnumerator Start()
        {
            // Ждем окончания кадра, чтобы сцена полностью инициализировалась
            yield return new WaitForEndOfFrame();

            if (SaveManager.Instance != null)
            {
                if (SaveManager.Instance.IsLoadingSave)
                {
                    // Применяем данные к Игроку и ко ВСЕМ NPC на сцене одновременно
                    SaveManager.Instance.ApplyLoadedData();
                }
                else
                {
                    // Новая игра: фиксируем стартовые координаты игрока
                    GameObject player = GameObject.FindWithTag("Player");
                    if (player != null)
                    {
                        SaveManager.Instance.UpdatePlayerPosition(player.transform.position);
                    }
                }
            }
        }
    }
}