using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppHeader : MonoBehaviour
{
    [Header(@"Header UI Configuration")]
    public TMPro.TMP_Text AppName;

    // Start is called before the first frame update
    void Start()
    {
        if (AppState.currentUser != null)
        {
            AppName.text = AppState.currentUser.AppName;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
