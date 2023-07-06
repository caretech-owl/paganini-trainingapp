using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

using UnityEngine.UI;

public class PermissionManager : MonoBehaviour
{
    public GameObject PermissionPanel;
    public GameObject CameraSupportedPanel;
    public GameObject CameraNotSupportedPanel;

    public GameObject PermissionButtonCamera;
    public GameObject PermissionButtonMikrophone;
    public GameObject PermissionButtonGPS;

    public Button CloseButton;

    public bool requireAllPermissions;


    public UnityEvent OnPanelClose;
    public UnityEvent OnPendingPermissions;

    private int NumAvailablePermissions = 0;
    private int NumGrantedPermissions = 0;


    // Start is called before the first frame update
    void Start()
    {

#if PLATFORM_ANDROID

        SetupPermission(PermissionButtonMikrophone, Permission.Microphone);
        SetupPermission(PermissionButtonCamera, Permission.Camera);
        SetupPermission(PermissionButtonGPS, Permission.FineLocation);


        if (NumGrantedPermissions < NumAvailablePermissions)
        {
            CameraSupportedPanel.SetActive(true);
            PermissionPanel.SetActive(true);
            OnPendingPermissions.Invoke();            
            CloseButton.onClick.AddListener(OnPanelCloseHandler);

            CloseButton.interactable = !requireAllPermissions;
        }
        else
        {
            OnPanelCloseHandler();
        }
#endif

    }

    // Update is called once per frame
    void Update()
    {
        if (NumGrantedPermissions == NumAvailablePermissions)
        {
            CloseButton.interactable = true;
        }

//#if PLATFORM_ANDROID
//        int nPermissions = 0;
//        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
//        {
//            PermissionButtonMikrophone.SetActive(false);
//            nPermissions++;
//        }
//        if (Permission.HasUserAuthorizedPermission(Permission.Camera))
//        {
//            PermissionButtonCamera.SetActive(false);
//            nPermissions++;
//        }
//        if (Permission.HasUserAuthorizedPermission(Permission.FineLocation))
//        {
//            PermissionButtonGPS.SetActive(false);
//            nPermissions++;
//        }
//        if (nPermissions == 3)
//        {
//            CloseButton.interactable = true;
//        }
//#endif
    }

    public void AskForPermissionMicrophone()
    {
        Permission.RequestUserPermission(Permission.Microphone);
        PermissionButtonMikrophone.SetActive(false);
    }

    public void AskForPermissionCamera()
    {
        Permission.RequestUserPermission(Permission.Camera);
        PermissionButtonCamera.SetActive(false);
    }

    public void AskForPermissionFineLocation()
    {
        Permission.RequestUserPermission(Permission.FineLocation);
        PermissionButtonGPS.SetActive(false);
    }

    private void OnPanelCloseHandler()
    {
        this.gameObject.SetActive(false);
        OnPanelClose.Invoke();
    }

    private bool SetupPermission(GameObject obj, string permission)
    {
        NumAvailablePermissions++;
        if (obj != null &&  !Permission.HasUserAuthorizedPermission(permission))
        {
            obj.SetActive(true);
            return true; 
        }
        NumGrantedPermissions++;

        return false;
    }

}
