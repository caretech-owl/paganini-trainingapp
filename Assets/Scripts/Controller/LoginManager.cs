using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using static PaganiniRestAPI;

public class LoginManager : MonoBehaviour
{

    public UnityEvent OnAlreadyLoggedIn;
    public UnityEvent OnNotLoggedIn;

    private void Awake()
    {
        DBConnector.Instance.Startup();
    }

    void Start()
    {
        RecheckLogin();
    }

    // Checks if currentUser is set in Appstate and handles it acordingly
    // IF not set, try to pull from local Database
    // IF not found go to UserLogin Scene
    public void RecheckLogin()
    {
        if (AppState.CurrentUser == null)
        {
            CheckAuthToken();
        }
        else
        {
            LoginStatusNextAction();
        }

    }

    public void Logout()
    {
        //DBConnector.Instance.TruncateTable<User>();

        AppState.CurrentUser = null;
        User.DeleteAll();
        AuthToken.DeleteAll();
    }

    private void CheckAuthToken()
    {
        var list = AuthToken.GetAll();

        AuthToken authToken = null;
        foreach (var token in list)
        {
            authToken = token;
            break;
        }

        if (authToken != null)
        {
            AppState.APIToken = authToken.ApiToken;
            ContinueUserSignIn();
        }
        else
        {
            LoginStatusNextAction();
        }


    }


    private void LoginStatusNextAction()
    {
        if (AppState.CurrentUser == null)
        {
            if (SceneManager.GetActiveScene().name != SceneSwitcher.UserLoginScene)
            {
                SceneManager.LoadScene(SceneSwitcher.UserLoginScene);
            }

            OnNotLoggedIn.Invoke();
        }
        else
        {
            OnAlreadyLoggedIn.Invoke();
        }

    }


    public void ContinueUserSignIn()
    {
        PaganiniRestAPI.User.GetProfile(GetUserProfileSucceed, GetUserProfileFailed);
    }

    private void GetUserProfileSucceed(UserAPI userApi)
    {        
        var list = User.GetAll( u => u.Id == userApi.user_id);

        User user = new User(userApi);

        // Keep the local copy if there is one
        if (list.Capacity > 0)
        {
            user.AppName = list[0].AppName;            
        }
        user.Insert();

        AppState.CurrentUser = user;

        LoginStatusNextAction();
    }

    private void GetUserProfileFailed(string errorMessage)
    {
        Debug.Log("Error getting Profile: " + errorMessage);

        AppState.CurrentUser = null;
        AppState.APIToken = null;
        LoginStatusNextAction();
    }




}
