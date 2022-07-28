using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginWithPin : MonoBehaviour
{
    public GameObject textinput;
    public void SendPinToAPI()
    {
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
    }

    private void GetUserProfileSucceed(UserAPI user)
    {
        AppState.currentUser = new User(user);
        DBConnector.Instance.Startup();
        DBConnector.Instance.GetConnection().Insert(AppState.currentUser);
        SceneManager.LoadScene(AppState.overviewScene);
    }

    private void GetUserProfileFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);
    }
}
