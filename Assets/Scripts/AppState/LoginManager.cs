using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
 
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
        }
        else if(SceneManager.GetActiveScene().name == AppState.UserLoginScene) 
        {
            SceneManager.LoadScene(AppState.overviewScene);
        }        
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
