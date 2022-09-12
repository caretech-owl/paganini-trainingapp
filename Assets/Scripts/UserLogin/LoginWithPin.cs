using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LoginWithPin : MonoBehaviour
{
    public GameObject textinput;
    public Button LoginButton;

    public void SendPinToAPI()
    {
        LoginButton.interactable = false;
        ServerCommunication.Instance.GetUserAuthentification(GetAuthSucceed, GetAuthFailed, System.Int32.Parse(textinput.GetComponent<TMP_InputField>().text));
    }

    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param String authtoken</param>
    private void GetAuthSucceed(APIToken token)
    {
        ServerCommunication.Instance.GetUserProfile(GetUserProfileSucceed, GetUserProfileFailed, token.apitoken);
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

        
    }

    private void GetUserProfileSucceed(UserAPI user)
    {
        AppState.currentUser = new User(user);
        DBConnector.Instance.Startup();
        DBConnector.Instance.GetConnection().Insert(AppState.currentUser);
        SceneManager.LoadScene(AppState.MyExploratoryRouteWalkScenes);
    }

    private void GetUserProfileFailed(string errorMessage)
    {
        LoginButton.interactable = true;
        Debug.LogError(errorMessage);
    }
}
