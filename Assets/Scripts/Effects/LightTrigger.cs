using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TriggerLightSwitch : MonoBehaviour
{
    [Header("Settings")]
    public Light2D targetLight;
    public float targetIntensity = 2.0f;

    private void Start()
    {
        if (targetLight != null)
        {
            targetLight.intensity = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (targetLight != null)
            {
                targetLight.intensity = targetIntensity;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (targetLight != null)
            {
                targetLight.intensity = 0f;
            }
        }
    }
}