using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingButton : MonoBehaviour
{

    public Button settingButton;
    // Start is called before the first frame update
    void Start()
    {
        settingButton.onClick.AddListener(() => this.SettingsButtonOnClick());

        AppState.lastScene = AppState.currentScene;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SettingsButtonOnClick()
    {
        SceneManager.LoadScene(AppState.settingsScene);
    }


}
