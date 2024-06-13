using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

public class XTapDetector : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Number of taps to detect")]
    public int tapsToDetect = 2; // Default to 2 taps
    [Tooltip("Maximum time between taps to consider it as part of the same sequence")]
    public float maxTapTimeframe = 0.5f; // Default to 0.5 seconds

    public UnityEvent XTapDetected;

    private int tapCount = 0;
    private float lastTapTime = 0f;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Calculate time since last tap
        float timeSinceLastTap = Time.time - lastTapTime;

        // If within tap timeframe, consider it part of the same sequence
        if (timeSinceLastTap < maxTapTimeframe)
        {
            tapCount++;

            // Check if taps to detect count is reached
            if (tapCount == tapsToDetect)
            {
                // Trigger XTapDetected event
                XTapDetected.Invoke();
                // Reset tap count
                tapCount = 0;
            }
        }
        else
        {
            // Reset tap count if time delay is exceeded
            tapCount = 1;
        }

        // Record the time of this tap
        lastTapTime = Time.time;
    }
}
