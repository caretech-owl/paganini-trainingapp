
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginSocialWorker : MonoBehaviour
{
    public GameObject InputFieldUsername;
    public GameObject InputFieldPassword;
    public GameObject ErrorMessage;

    public ButtonPrefab LoginButton;
    public UnityEvent OnNotLoggedIn;
    public UnityEvent OnLoginSucceed;
    public UnityEvent OnLoginFail;

    private void Awake()
    {
        DBConnector.Instance.Startup();
    }

    // Start is called before the first frame update
    void Start()
    {
        //if (AppState.SWAPIToken != null && AppState.SWAPIToken.Trim() != "")
        //{
        //    PaganiniRestAPI.SocialWorker.GetProfile(GetProfileSucceed, VerifyExistingAuthFailed);
        //}
        //else
        //{
        //    OnNotLoggedIn?.Invoke();
        //}

        // We do not check the credentials again
        if (AppState.SWAPIToken != null && AppState.CurrenSocialWorker != null)
        {
            OnLoginSucceed?.Invoke();
        } else
        {
            OnNotLoggedIn?.Invoke();
        }
    }

    public void SendCredentialsToAPI()
    {
        ErrorMessage?.SetActive(false);
        LoginButton.RenderBusyState(true);

        PaganiniRestAPI.SocialWorker.Authenticate(InputFieldUsername.GetComponent<TMP_InputField>().text,
                                                  InputFieldPassword.GetComponent<TMP_InputField>().text,
                                                  GetAuthSucceed, GetAuthFailed);
    }

    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param String authtoken</param>
    private void GetAuthSucceed(AuthTokenAPI token)
    {
        AppState.SWAPIToken = token.apitoken;
        PaganiniRestAPI.SocialWorker.GetProfile(GetProfileSucceed, GetAuthFailed);  
    }

    private void GetProfileSucceed(SocialWorkerAPIResult profile)
    {
        AppState.CurrenSocialWorker = new SocialWorker(profile);
        OnLoginSucceed?.Invoke();
    }

    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    private void GetAuthFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);
        LoginButton.RenderBusyState(false);
        ErrorMessage.SetActive(true);

        Assets.ErrorHandlerSingleton.GetErrorHandler().AddNewError("AuthFailed", errorMessage);

        OnLoginFail.Invoke();
    }

    private void VerifyExistingAuthFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);
        OnNotLoggedIn?.Invoke();
    }


}
