using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoalReached : MonoBehaviour
{

    public MainMenu mainMenu;

    public Button doneButton;

    // Start is called before the first frame update
    void Start()
    {
        this.mainMenu.DisablePauseButton();
        this.mainMenu.DisableMuteButton();
        this.mainMenu.DisableInkognitoButton();

        this.doneButton.onClick.AddListener(() => this.DoneButtonOnClick());

        // Set current scene to last scene for navigation
        AppState.lastScene = AppState.currentScene;
        AppState.currentScene = SceneSwitcher.goalReachedScene;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void DoneButtonOnClick()
    {
        this.mainMenu.EnablePauseButton();
        this.mainMenu.EnableMuteButton();
        this.mainMenu.EnableInkognitoButton();
        SceneManager.LoadScene(SceneSwitcher.startScene);
    }
}
