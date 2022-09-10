using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class PermissionManager : MonoBehaviour
{
    public GameObject PermissionPanel;
    public GameObject PermissionButtonCamera;
    public GameObject PermissionButtonMikrophone;
    public GameObject PermissionButtonGPS;

    public GameObject LandingPagePanel;


    // Start is called before the first frame update
    void Start()
    {
#if PLATFORM_ANDROID
        bool askPermission = false;
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            PermissionButtonMikrophone.SetActive(true);
            askPermission = true;
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            PermissionButtonCamera.SetActive(true);
            askPermission = true;
        }
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            PermissionButtonGPS.SetActive(true);            
            askPermission = true;
        }

        if (askPermission)
        {
            PermissionPanel.SetActive(true);
            LandingPagePanel.SetActive(false);
        }

#endif
    }

    // Update is called once per frame
    void Update()
    {
#if PLATFORM_ANDROID
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            PermissionButtonMikrophone.SetActive(false);
        }
        if (Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            PermissionButtonCamera.SetActive(false);
        }
        if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            PermissionButtonGPS.SetActive(false);
        }
#endif
    }

    public void AskForPermissionMicrophone()
    {
        Permission.RequestUserPermission(Permission.Microphone);
    }

    public void AskForPermissionCamera()
    {
        Permission.RequestUserPermission(Permission.Camera);
    }

    public void AskForPermissionFineLocation()
    {
        Permission.RequestUserPermission(Permission.FineLocation);
    }

}
