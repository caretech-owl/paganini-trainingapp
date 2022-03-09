using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VideoHelp : MonoBehaviour
{

    public GameObject settingsButton;

    public MainMenu mainMenu;

    public NextStep nextStep;

    public GameObject videoObject;

    public Button endCallButton;

    // Start is called before the first frame update
    void Start()
    {
        this.mainMenu.DisablePauseButton();
        this.mainMenu.DisableCallButton();
        this.endCallButton.onClick.AddListener(() => HangUpButtonOnClick());

        // Set current scene to last scene for navigation
        AppState.lastScene = AppState.currentScene;
        AppState.currentScene = AppState.videoHelpScene;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void HangUpButtonOnClick()
    {
        this.mainMenu.EnableCallButton();
        this.mainMenu.EnablePauseButton();
        SceneManager.LoadScene(AppState.wegeFührungScene);

        // TODO CALL IMPLEMENTATION

    }
}
