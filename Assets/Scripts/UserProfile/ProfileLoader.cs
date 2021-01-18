using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Debug.Log("nothing to restore");
    }

    /// <summary>
    /// Saves userProfile zo PlayerPrefs
    /// </summary>
    void SaveUserData()
    {
        profilesting = JsonUtility.ToJson(userProfile);
        PlayerPrefs.SetString("userProfile", profilesting);
        Debug.Log(profilesting);
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
    }
    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    private void APICallFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);
    }
}
