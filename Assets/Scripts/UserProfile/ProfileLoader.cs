using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileLoader : MonoBehaviour
{
    public GameObject ProfileSection;
    public UserProfile userProfile;

    private string profilesting;
    // Start is called before the first frame update
    void Start()
    {
        userProfile = new UserProfile();
        RestoreUserData();
    }

    /// <summary>
    /// Reads back in the Profile from Playerprefs and Saves it in the Object
    /// </summary>
    void RestoreUserData()
    {
        profilesting = PlayerPrefs.GetString("userProfile");
        UserProfile readProfile = JsonUtility.FromJson<UserProfile>(profilesting);
        if (readProfile != null)
        {
            userProfile = readProfile;
        }
        DisplayProfile();
    }

    /// <summary>
    /// Deletes Profile Data
    /// </summary>
    public void DeleteUserData()
    {
        PlayerPrefs.DeleteKey("userProfile");
        userProfile = new UserProfile();
        RestoreUserData();
    }

    /// <summary>
    /// Saves userProfile zo PlayerPrefs
    /// </summary>
    void SaveUserData()
    {
        profilesting = JsonUtility.ToJson(userProfile);
        PlayerPrefs.SetString("userProfile", profilesting);
    }
    public void LoginToServer()
    {
        ServerCommunication.Instance.GetUserProfile(APICallSucceed, APICallFailed);
    }

    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param name="userProfile">UserProfile Object</param>
    private void APICallSucceed(UserProfile userProfile)
    {
        this.userProfile = userProfile;
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
                        if (userProfile.ben_id != 0)
                            df.text = userProfile.ben_id.ToString();
                        else
                            df.text = null;
                        break;
                    case "DFUsername":
                        if (userProfile.ben_id != 0)
                            df.text = userProfile.ben_benutzername;
                        else
                            df.text = null;
                        break;
                    case "DFMnemonic":
                        if (userProfile.ben_id != 0)
                            df.text = userProfile.ben_mnemonic_token;
                        else
                            df.text = null;
                        break;


                }

            }
        }
    }
}
