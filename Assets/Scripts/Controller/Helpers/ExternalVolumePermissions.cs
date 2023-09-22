using UnityEngine;
using System.Collections;
using UnityEngine.Android; // Add this namespace for Android permissions
using UnityEngine.Events;

public class ExternalVolumePermissions : MonoBehaviour
{
    public UnityEvent OnPermissionGranted;
    public UnityEvent OnPermissionDenied;

    private bool hasWritePermission = false;
    private Coroutine permissionCoroutine;

    private void Start()
    {
        // Check if we already have the write permission
        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            TriggerPermissionGranted();
        }
        else
        {
            // Request write permission from the user
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
            permissionCoroutine = StartCoroutine(CheckWritePermissionCoroutine());
        }
    }

    // Callback for Android permission request
    private IEnumerator CheckWritePermissionCoroutine()
    {
        yield return new WaitForSeconds(1f); // Wait a short while after requesting the permission

        // Check if the user has granted the write permission
        if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite) &&
            Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            TriggerPermissionGranted();            
        }
        else
        {
            // Handle the case where the user denied the write permission
            // You can display a message or take other appropriate actions
            Debug.Log("Write permission denied. Cannot write to USB device.");

            TriggerPermissionDenied();
        }
    }

    // Call this function when you want to write to the USB device
    public void TriggerPermissionGranted()
    {
        OnPermissionGranted?.Invoke();
        if (permissionCoroutine != null)
        {
            StopCoroutine(permissionCoroutine);
        }
    }

    public void TriggerPermissionDenied()
    {
        OnPermissionDenied?.Invoke();
        if (permissionCoroutine != null)
        {
            StopCoroutine(permissionCoroutine);
        }
    }
}


