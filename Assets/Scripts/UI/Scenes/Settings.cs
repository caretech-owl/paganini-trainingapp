using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{

    public Button BackButton;

    // Start is called before the first frame update
    void Start()
    {
        BackButton.onClick.AddListener(() => this.BackButtonOnClick());

        // Set current scene to last scene for navigation
        AppState.lastScene = AppState.currentScene;
        AppState.currentScene = AppState.settingsScene;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void BackButtonOnClick()
    {
        Debug.Log("LOAD " + AppState.lastScene);
        SceneManager.LoadScene(AppState.lastScene);
    }
}
