using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

using UnityEngine.UI;

public class PermissionManager : MonoBehaviour
{
    public GameObject PermissionPanel;
    public GameObject PermissionButtonCamera;
    public GameObject PermissionButtonMikrophone;
    public GameObject PermissionButtonGPS;

    public Button CloseButton;

    public bool requireAllPermissions;


    public UnityEvent OnPanelClose;
    public UnityEvent OnPendingPermissions;


    // Start is called before the first frame update
    void Start()
    {
        bool askPermission = false;
#if PLATFORM_ANDROID

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

#endif
        if (askPermission)
        {
            PermissionPanel.SetActive(true);
            OnPendingPermissions.Invoke();            
            CloseButton.onClick.AddListener(OnPanelCloseHandler);

            CloseButton.interactable = !requireAllPermissions;
        }
        else
        {
            OnPanelCloseHandler();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
#if PLATFORM_ANDROID
        int nPermissions = 0;
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            PermissionButtonMikrophone.SetActive(false);
            nPermissions++;
        }
        if (Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            PermissionButtonCamera.SetActive(false);
            nPermissions++;
        }
        if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            PermissionButtonGPS.SetActive(false);
            nPermissions++;
        }
        if (nPermissions == 3)
        {
            CloseButton.interactable = true;
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

    private void OnPanelCloseHandler()
    {
        this.gameObject.SetActive(false);
        OnPanelClose.Invoke();
    }

}
