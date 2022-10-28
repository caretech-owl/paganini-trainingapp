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

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateLocalProfile()
    {
        User user = AppState.currentUser;
        user.AppName = AppName.text;
        
        DBConnector.Instance.GetConnection().InsertOrReplace(user);
    }

    public void TriggerLocalProfileVerification()
    {

        List<User> profile = DBConnector.Instance.GetConnection().Query<User>("Select * FROM User where AppName is not NULL AND Id = " + AppState.currentUser.Id );
        if (profile.Capacity > 0)
        {
            OnLocalProfileFound.Invoke();
        }
        else
        {
            OnLocalProfileNotFound.Invoke();
        }

    }

}
