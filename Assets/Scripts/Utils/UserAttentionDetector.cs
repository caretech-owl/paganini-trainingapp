using UnityEngine;

public class UserAttentionDetector : MonoBehaviour
{
    // Thresholds for detecting user attention
    public float sidewaysAngleThreshold = 30f; // Angle threshold for sideways tilting in degrees
    public float upDownAngleThreshold = 15f; // Angle threshold for up/down tilting in degrees

    // Reference orientation (in world space) when the user is looking at the phone
    public Vector3 referenceOrientation = Vector3.forward; // Default is the forward direction

    // Time window for continuous attention detection (in seconds)
    public float attentionTimeWindow = 5f;

    // Delay before resetting attention timer (in seconds)
    public float resetDelay = 1f;

    // Variables to store previous sensor data
    private Quaternion referenceRotation;

    // Timer variables
    private float attentionTimer = 0f;
    private bool isLooking = false;
    private float notLookingTimer = 0f;


    // Start is called before the first frame update
    void Start()
    {
        // Set reference rotation based on the reference orientation
        referenceRotation = Quaternion.LookRotation(referenceOrientation);

        // Check if accelerometer is supported
        if (!SystemInfo.supportsAccelerometer)
        {
            Debug.LogError("Accelerometer is not supported on this device");
            enabled = false; // Disable script if accelerometer is not supported
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get current accelerometer data
        Vector3 gravity = Input.acceleration;

        // Convert gravity vector to rotation
        Quaternion currentRotation = Quaternion.FromToRotation(-Vector3.forward, gravity);

        // Calculate angle between current rotation and reference rotation
        float angle = Quaternion.Angle(referenceRotation, currentRotation);

        // Check if the angle change exceeds the threshold based on the orientation
        float angleThreshold = (Mathf.Abs(Vector3.Dot(currentRotation * Vector3.up, Vector3.up)) < 0.5f) ? sidewaysAngleThreshold : upDownAngleThreshold;

        // Check if the angle change exceeds threshold
        if (angle < angleThreshold)
        {
            // Increment attention timer if user is continuously looking at the screen
            if (!isLooking)
            {
                isLooking = true;
                attentionTimer = 0f;
                userCurrentlyLooking = false;
            }
            else
            {
                attentionTimer += Time.deltaTime;
                if (attentionTimer >= attentionTimeWindow)
                {
                    // Perform action when user has been looking at the screen for X seconds
                    if (!userCurrentlyLooking)
                    {
                        TriggerUserLookingDetected();
                        userCurrentlyLooking = true;
                    }
                }
            }
        }
        else
        {
            // Increment not looking timer
            notLookingTimer += Time.deltaTime;

            // Reset attention timer if user is not looking at the screen for the specified delay duration
            if (notLookingTimer >= resetDelay)
            {
                isLooking = false;
                attentionTimer = 0f;
                notLookingTimer = 0f;

                if (userCurrentlyLooking)
                {
                    TriggerUserNotLookingDetected();
                    userCurrentlyLooking = false;
                }
            }
        }
    }

    private bool userCurrentlyLooking = false;
    private void TriggerUserLookingDetected()
    {
        Debug.LogWarning("[Looking ON] User has been looking at the screen for " + attentionTimeWindow + " seconds");
    }

    private void TriggerUserNotLookingDetected()
    {
        Debug.LogWarning("[Looking OFF] User has not been looking at the screen for  " + notLookingTimer + " seconds");
    }
}
