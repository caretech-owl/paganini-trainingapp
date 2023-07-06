using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class ProfileDataHandler : MonoBehaviour
{

    [Header(@"Profile form Configuration")]
    public TMPro.TMP_InputField AppName;

    public UnityEvent OnLocalProfileFound;
    public UnityEvent OnLocalProfileNotFound;


    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void UpdateLocalProfile()
    {
        User user = AppState.CurrentUser;
        user.AppName = AppName.text;

        //DBConnector.Instance.GetConnection().InsertOrReplace(user);

        user.Insert();
    }

    public void TriggerLocalProfileVerification()
    {

        //List<User> profile = DBConnector.Instance.GetConnection().Query<User>("Select * FROM User where AppName is not NULL AND Id = " + AppState.CurrentUser.Id );
        //if (profile.Capacity > 0)

        // Let's check if there is a name
        if (AppState.CurrentUser.AppName == null || AppState.CurrentUser.AppName.Trim().Length == 0)
        {
            OnLocalProfileNotFound.Invoke();
        }
        else
        {            
            OnLocalProfileFound.Invoke();
        }

    }

}
