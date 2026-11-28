using UnityEngine;

/// <summary>
/// Makes the Heart pickup bob smoothly up/down and pulsate like a beating heart.
/// </summary>
public class HeartVisual : MonoBehaviour
{
    [Header("Hover Bobbing")]
    [SerializeField] private float bobFrequency = 3f;
    [SerializeField] private float bobAmplitude = 0.08f;

    [Header("Heartbeat Pulse")]
    [SerializeField] private float pulseFrequency = 4.5f;
    [SerializeField] private float pulseScaleAmount = 0.12f;

    private Vector3 initialScale;
    private float initialLocalY;

    private void Start()
    {
        initialScale = transform.localScale;
        initialLocalY = transform.localPosition.y;
    }

    private void Update()
    {
        // 1. Bobbing
        float newY = initialLocalY + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);

        // 2. Beating pulse
        float scaleFactor = 1f + Mathf.Sin(Time.time * pulseFrequency) * pulseScaleAmount;
        transform.localScale = initialScale * scaleFactor;
    }
}
