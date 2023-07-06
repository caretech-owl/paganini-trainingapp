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
        LoadFromAppSession();
    }

    public void LoadFromAppSession()
    {
        if (AppState.CurrentUser != null)
        {
            AppName.text = AppState.CurrentUser.AppName;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
