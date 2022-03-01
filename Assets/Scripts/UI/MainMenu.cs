using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public GameObject phoneButton;

    public GameObject pauseButton;

    public GameObject muteButton;

    public GameObject inkognitoButton;

    public Sprite playButtonImage;

    // Start is called before the first frame update
    void Start()
    {
        this.phoneButton = gameObject.transform.Find("CallButton").gameObject;
        this.pauseButton = gameObject.transform.Find("PauseButton").gameObject;
        this.muteButton = gameObject.transform.Find("MuteButton").gameObject;
        this.inkognitoButton = gameObject.transform.Find("InkognitoButton").gameObject;

        var button = this.phoneButton.GetComponentsInChildren<Button>();
        button[0].onClick.AddListener(() => this.PhoneButtonOnClick());

        button = this.pauseButton.GetComponentsInChildren<Button>();
        button[0].onClick.AddListener(() => this.PauseButtonOnClick(false));

        button = this.muteButton.GetComponentsInChildren<Button>();
        button[0].onClick.AddListener(() => this.MuteButtonOnClick());

        button = this.inkognitoButton.GetComponentsInChildren<Button>();
        button[0].onClick.AddListener(() => this.InkognitoButtonOnClick());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangePauseToContinueButton()
    {
        var mainButtons = gameObject.GetComponentsInChildren<Button>();
        mainButtons[1].onClick.RemoveAllListeners();
        mainButtons[1].GetComponent<Image>().sprite = playButtonImage;
        mainButtons[1].onClick.AddListener(() => this.PauseButtonOnClick(true));
    }

    public void ChangeContinueToPauseButton()
    {
        var mainButtons = gameObject.GetComponentsInChildren<Button>();
        mainButtons[1].onClick.RemoveAllListeners();
        mainButtons[1].GetComponent<Image>().sprite = playButtonImage;
        mainButtons[1].onClick.AddListener(() => this.PauseButtonOnClick(false));
    }
    public void EnableCallButton()
    {
        var mainButtons = gameObject.GetComponentsInChildren<Button>();
        mainButtons[0].gameObject.SetActive(true);
    }

    public void DisableCallButton()
    {
        var mainButtons = gameObject.GetComponentsInChildren<Button>();
        mainButtons[0].gameObject.SetActive(false);
    }

    public void EnablePauseButton()
    {
        var mainButtons = gameObject.GetComponentsInChildren<Button>();
        mainButtons[1].gameObject.SetActive(true);
    }

    public void DisablePauseButton()
    {
        var mainButtons = gameObject.GetComponentsInChildren<Button>();
        mainButtons[1].gameObject.SetActive(false);
    }

    public void EnableMuteButton()
    {
        var mainButtons = gameObject.GetComponentsInChildren<Button>();
        mainButtons[1].gameObject.SetActive(true);
    }

    public void DisableMuteButton()
    {
        var mainButtons = gameObject.GetComponentsInChildren<Button>();
        mainButtons[1].gameObject.SetActive(false);
    }

    public void EnableInkognitoButton()
    {
        var mainButtons = gameObject.GetComponentsInChildren<Button>();
        mainButtons[1].gameObject.SetActive(true);
    }

    public void DisableInkognitoButton()
    {
        var mainButtons = gameObject.GetComponentsInChildren<Button>();
        mainButtons[1].gameObject.SetActive(false);
    }


    public GameObject GetButtonByName(string buttonName)
    {
        return gameObject.transform.Find(buttonName).gameObject;
    }

    private void PhoneButtonOnClick()
    {
        SceneManager.LoadScene(AppState.videoHelpScene);
    }

    private void PauseButtonOnClick(bool isContinue)
    {
        if (isContinue)
        {
            this.ChangeContinueToPauseButton();
            SceneManager.LoadScene(AppState.wegeFührungScene);
        }
        else
        {
            SceneManager.LoadScene(AppState.pausedScene);

        }
    }

    private void MuteButtonOnClick()
    {
        Debug.Log("MuteButtonOnClick");

    }

    private void InkognitoButtonOnClick()
    {
        Debug.Log("InkognitoButtonOnClick");

    }
}
