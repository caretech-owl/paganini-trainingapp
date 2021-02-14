using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This class is responsible for handling The Local UserProfile Data
/// </summary>
public class ProfileHandler : MonoBehaviour
{
    /// <summary>
    /// Game Object to Display Profile in
    /// </summary>
    public GameObject ProfileSection;
    /// <summary>
    /// Object with the Current userProfile
    /// </summary>
    public UserProfile userProfile;
    /// <summary>
    /// Object with the Current userProfile
    /// </summary>
    public AuthToken authToken;

    // Start is called before the first frame update
    void Start()
    {
        DBConnector.Instance.Startup();
        //Mock
        Login();
        RestoreUserData();
    }

    void Login()
    {
        List<AuthToken> token = DBConnector.Instance.GetConnection().Query<AuthToken>("Select * FROM AuthToken");
        if (token.Capacity > 0)
            authToken = token[0];
        else
        {
            authToken = new AuthToken();
            authToken.ben_authtoken = "123";
            DBConnector.Instance.GetConnection().InsertOrReplace(authToken);
        }
    }

    /// <summary>
    /// Reads back in the Profile from SQLite and Saves it in the Object
    /// </summary>
    void RestoreUserData()
    {
        List<UserProfile> profile = DBConnector.Instance.GetConnection().Query<UserProfile>("Select * FROM UserProfile");
        if (profile.Capacity > 0)
            userProfile = profile[0];
        else
            userProfile = null;
        DisplayProfile();
    }

    /// <summary>
    /// Deletes Profile Data
    /// </summary>
    public void DeleteUserData()
    {
        DBConnector.Instance.GetConnection().DeleteAll<UserProfile>();
        userProfile = new UserProfile();
        RestoreUserData();
    }

    /// <summary>
    /// Saves userProfile to SQLite
    /// </summary>
    void SaveUserData()
    {
        if (userProfile != null)
            DBConnector.Instance.GetConnection().InsertOrReplace(userProfile);
    }
    /// <summary>
    /// Calls Rest API to get current UserProfile
    /// </summary>
    public void LoginToServer()
    {
        ServerCommunication.Instance.GetUserProfile(APICallSucceed, APICallFailed, authToken.ben_authtoken);
    }

    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param name="userProfile">UserProfile Object</param>
    private void APICallSucceed(UserProfileAPI userProfile)
    {
        this.userProfile = new UserProfile(userProfile);
        SaveUserData();
        RestoreUserData();
    }

    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    private void APICallFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);
    }

    /// <summary>
    /// Prints the UserProfile to the GUI
    /// </summary>
    private void DisplayProfile()
    {
        if (ProfileSection != null)
        {
            var list = ProfileSection.GetComponentsInChildren<Text>();
            foreach (var df in list)
            {
                switch (df.name)
                {
                    case "DFUserID":
                        if (userProfile != null)
                            df.text = userProfile.ben_id.ToString();
                        else
                            df.text = null;
                        break;
                    case "DFUsername":
                        if (userProfile != null)
                            df.text = userProfile.ben_benutzername;
                        else
                            df.text = null;
                        break;
                    case "DFMnemonic":
                        if (userProfile != null)
                            df.text = userProfile.ben_mnemonic_token;
                        else
                            df.text = null;
                        break;
                    case "DFAuth":
                        if (authToken != null)
                            df.text = authToken.ben_authtoken;
                        else
                            df.text = null;
                        break;

                }

            }
        }
    }
}
