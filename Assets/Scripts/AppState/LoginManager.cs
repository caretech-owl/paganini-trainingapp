using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

public class LoginManager : MonoBehaviour
{

    public UnityEvent OnAlreadyLoggedIn;
    public UnityEvent OnNotLoggedIn;

    void Start()
    {
        RecheckLogin();
    }

    // Checks if currentUser is set in Appstate and handles it acordingly
    // IF not set, try to pull from local Database
    // IF not found go to UserLogin Scene
    public void RecheckLogin()
    {
        if (AppState.currentUser == null)
        {
            DBConnector.Instance.Startup();
            ReadUserFromDatabase();
        }
        if (AppState.currentUser == null)
        {
            if (SceneManager.GetActiveScene().name != AppState.UserLoginScene)
            {
                SceneManager.LoadScene(AppState.UserLoginScene);
            }

            OnNotLoggedIn.Invoke();
        }
        else 
        {
            OnAlreadyLoggedIn.Invoke();
        }        
    }

    public void Logout()
    {
        DBConnector.Instance.TruncateTable<User>();
        AppState.currentUser = null;
    }

    void ReadUserFromDatabase()
    {
        List <User> user = DBConnector.Instance.GetConnection().Query<User>("Select * FROM User");
        if (user.Capacity > 0)
            AppState.currentUser  =user[0];
        else
            AppState.currentUser = null;
    }
}
