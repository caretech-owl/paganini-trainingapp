using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class LoginWithPin : MonoBehaviour
{
    public GameObject textinput;
    public Button LoginButton;
    public UnityEvent OnLoginSucceed;
    public UnityEvent OnLoginFail;

    private void Awake()
    {
        DBConnector.Instance.Startup();
    }

    public void SendPinToAPI()
    {
        LoginButton.interactable = false;

        int pin = System.Int32.Parse(textinput.GetComponent<TMP_InputField>().text);
        PaganiniRestAPI.User.Authenticate(pin, GetAuthSucceed, GetAuthFailed);
    }


    public void ContinueUserSignIn(AuthTokenAPI token)
    {
        PaganiniRestAPI.User.GetProfile(GetUserProfileSucceed, GetUserProfileFailed);
    }


    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param String authtoken</param>
    private void GetAuthSucceed(AuthTokenAPI token)
    {
        // Clear out old auth tokens
        AuthToken.DeleteAll();

        // Let's save the new token
        var authToken = new AuthToken(token);
        authToken.Insert();
        AppState.APIToken = authToken.ApiToken;

        ContinueUserSignIn(token);
    }

    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    private void GetAuthFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);
        LoginButton.interactable = true;

        Assets.ErrorHandlerSingleton.GetErrorHandler().AddNewError("AuthFailed", errorMessage);

        OnLoginFail.Invoke();
    }

    private void GetUserProfileSucceed(UserAPI userApi)
    {

        var user = new User(userApi);
        user.Insert();

        AppState.CurrentUser = user;

        OnLoginSucceed.Invoke();
    }

    private void GetUserProfileFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);
        LoginButton.interactable = true;        

        //Assets.ErrorHandlerSingleton.GetErrorHandler().AddNewError("AuthFailed", errorMessage);

        OnLoginFail.Invoke();
    }
}
