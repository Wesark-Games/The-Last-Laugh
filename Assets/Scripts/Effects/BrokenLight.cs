using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BrokenLightFlicker : MonoBehaviour
{
    [Header("Settings")]
    public float minTime = 0.05f;
    public float maxTime = 0.4f;
    public float targetIntensity = 2.0f;

    private Light2D light2D;
    private float timer;
    private bool isOn = true;

    void Start()
    {
        light2D = GetComponent<Light2D>();
        SetRandomTimer();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            isOn = !isOn;
            light2D.intensity = isOn ? targetIntensity : 0f;
            
            SetRandomTimer();
        }
    }

    void SetRandomTimer()
    {
        timer = Random.Range(minTime, maxTime);
    }
}