using UnityEngine;

public class MoveEffect : MonoBehaviour
{
    public Transform destinationTransform;
    //public float destinationY = 0f; // Y position of the destination
    public float movementDuration = 1f; // Duration of movement in seconds

    private Vector3 originalPosition; // Original position of the object
    private Vector3 endPosition; // Destination position
    private float elapsedTime = 0f; // Elapsed time since movement started

    private bool isMovingToDestination = false; // Flag to indicate if moving to destination is in progress
    private bool isReturningToOriginal = false; // Flag to indicate if returning to original position is in progress
    private bool isMoving = false;

    private void Start()
    {
        // Store the original position of the object
        originalPosition = transform.position;
    }

    private void Update()
    {
        if (!isMoving) return;

        if (isMovingToDestination)
        {
            MoveToDestination();
        }
        else if (isReturningToOriginal)
        {
            ReturnToOriginal();
        }
    }
    
    // Function to trigger the movement effect (toggle between destination and original position)
    public void TriggerMoveEffect()
    {
        // Toggle between moving to destination and returning to original position
        if (!isMovingToDestination)
        {
            // Start moving to destination if not already moving
            endPosition = destinationTransform.position;//new Vector3(originalPosition.x, destinationY, originalPosition.z);
            isMovingToDestination = true;
            isReturningToOriginal = false;
        }
        else
        {
            // Start returning to original position if already moving to destination
            //endPosition = originalPosition;
            isMovingToDestination = false;
            isReturningToOriginal = true;
        }
        isMoving = true;
        elapsedTime = 0f; // Reset elapsed time
    }

    public void TriggerMoveToOriginal()
    {
        isMovingToDestination = false;
        isReturningToOriginal = true;
        isMoving = true;
        elapsedTime = 0f; // Reset elapsed time
    }

    public bool IsAtOriginalPosition()
    {
        return transform.position == originalPosition;
    }

    // Function to move the object to the destination
    private void MoveToDestination()
    {
        // Increment the elapsed time
        elapsedTime += Time.deltaTime;

        // Calculate the interpolation factor (0 to 1)
        float t = Mathf.Clamp01(elapsedTime / movementDuration);

        // Interpolate between the original and destination positions using Lerp
        transform.position = Vector3.Lerp(originalPosition, endPosition, t);

        // Check if the movement is complete
        if (t >= 1f)
        {
            // Movement to destination is complete
            isMoving = false;
            elapsedTime = 0f;
        }
    }

    // Function to return the object to its original position
    private void ReturnToOriginal()
    {
        // Increment the elapsed time
        elapsedTime += Time.deltaTime;

        // Calculate the interpolation factor (0 to 1)
        float t = Mathf.Clamp01(elapsedTime / movementDuration);

        // Interpolate between the destination and original positions using Lerp
        transform.position = Vector3.Lerp(endPosition, originalPosition, t);

        // Check if the movement is complete
        if (t >= 1f)
        {
            // Movement to original position is complete
            isMoving = false;
            elapsedTime = 0f;
        }
    }

    // Function to reset the object to its original position
    public void ResetOriginal()
    {
        // Reset the position to the original position
        transform.position = originalPosition;

        // Reset all related variables
        endPosition = originalPosition;
        elapsedTime = 0f;
        isMovingToDestination = false;
        isReturningToOriginal = false;
    }


}


