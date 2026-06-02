using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightCulling : MonoBehaviour
{
    private Camera cam;
    
    [Header("Настройки оптимизации")]
    [Tooltip("Как часто проверять свет (в секундах). Не нужно проверять каждый кадр.")]
    public float checkInterval = 0.2f;
    
    [Tooltip("Дополнительный запасной отступ за пределами экрана, чтобы свет не включался прямо на глазах у игрока")]
    public float bufferDistance = 5f;

    private Light2D[] allLights;
    private float timer;

    void Start()
    {
        cam = GetComponent<Camera>();
        // Находим все источники света на сцене при старте
        FindAllLights();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            CullLights();
        }
    }

    // Метод можно вызывать вручную, если ты спавнишь свет во время игры
    public void FindAllLights()
    {
        allLights = Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None);
    }

    void CullLights()
    {
        if (allLights == null || allLights.Length == 0) return;

        // Получаем границы видимости камеры в мировых координатах
        Vector3 camPos = cam.transform.position;
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // Границы экрана с учетом буфера
        float minX = camPos.x - camWidth - bufferDistance;
        float maxX = camPos.x + camWidth + bufferDistance;
        float minY = camPos.y - camHeight - bufferDistance;
        float maxY = camPos.y + camHeight + bufferDistance;

        foreach (Light2D light in allLights)
        {
            if (light == null) continue;

            // Пропускаем глобальный свет, он должен работать всегда
            if (light.lightType == Light2D.LightType.Global) continue;

            Vector3 lightPos = light.transform.position;

            // Проверяем, входит ли свет в границы экрана + буфер
            bool isVisible = (lightPos.x >= minX && lightPos.x <= maxX &&
                              lightPos.y >= minY && lightPos.y <= maxY);

            // Если свет должен быть выключен, но он включен (или наоборот) — меняем состояние
            if (light.enabled != isVisible)
            {
                light.enabled = isVisible;

                // ДОПОЛНИТЕЛЬНО: Если на объекте со светом висят твои скрипты мерцания 
                // (TvLightFlicker или FlickeringLight), отключаем и их, чтобы не тратить процессорное время
                var flickerScript1 = light.GetComponent<MonoBehaviour>(); // Ищет любые кастомные скрипты
                if (flickerScript1 != null && flickerScript1 != this)
                {
                    flickerScript1.enabled = isVisible;
                }
            }
        }
    }
}