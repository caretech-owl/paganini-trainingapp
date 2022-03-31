using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// using MainMenu.MainMenu;

public class PausedView : MonoBehaviour
{

    public GameObject mainMenue;

    public GameObject nextStep;

    public GameObject continueButton;

    // Start is called before the first frame update
    void Start()
    {
        this.continueButton = gameObject.transform.Find("ContinueButton").gameObject;

        var button = this.continueButton.GetComponentInChildren<Button>();
        button.onClick.AddListener(() => this.ContinueButtonOnClick());

        MainMenu main = gameObject.GetComponentInChildren<MainMenu>();
        main.ChangePauseToContinueButton();

        // Set current scene to last scene for navigation
        AppState.lastScene = AppState.currentScene;
        AppState.currentScene = AppState.pausedScene;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ContinueButtonOnClick()
    {
        SceneManager.LoadScene(AppState.wegeFührungScene);
    }
}
