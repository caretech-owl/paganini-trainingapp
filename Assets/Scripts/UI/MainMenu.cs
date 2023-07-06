using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public Button phoneButton;

    public Button pauseButton;

    public Button muteButton;

    public Button inkognitoButton;

    public Sprite playButtonImage;

    public Sprite mutedIcon;

    public Sprite notMutedIcon;

    public Sprite incognitoIcon;

    public Sprite notIncognitoIcon;

    // Start is called before the first frame update
    void Start()
    {
        this.phoneButton.onClick.AddListener(() => this.PhoneButtonOnClick());
        this.pauseButton.onClick.AddListener(() => this.PauseButtonOnClick(false));
        this.muteButton.onClick.AddListener(() => this.MuteButtonOnClick());
        this.inkognitoButton.onClick.AddListener(() => this.InkognitoButtonOnClick());

        this.UpdateMuteButtonIcon(AppState.isMute);
        this.UpdateIncocnitoButtonIcon(AppState.isIncognito);
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
        SceneManager.LoadScene(SceneSwitcher.videoHelpScene);
    }

    private void PauseButtonOnClick(bool isContinue)
    {
        if (isContinue)
        {
            this.ChangeContinueToPauseButton();
            SceneManager.LoadScene(SceneSwitcher.wegeFührungScene);
        }
        else
        {
            SceneManager.LoadScene(SceneSwitcher.pausedScene);

        }
    }

    private void UpdateMuteButtonIcon(bool state)
    {
        if (state)
        {
            this.muteButton.GetComponent<Image>().sprite = mutedIcon;

        }
        else
        {
            this.muteButton.GetComponent<Image>().sprite = notMutedIcon;
        }
    }
    private void UpdateIncocnitoButtonIcon(bool state)
    {
        if (state)
        {
            this.inkognitoButton.GetComponent<Image>().sprite = incognitoIcon;

        }
        else
        {
            this.inkognitoButton.GetComponent<Image>().sprite = notIncognitoIcon;
        }
    }

    private void MuteButtonOnClick()
    {
        AppState.isMute = !AppState.isMute;
        this.UpdateMuteButtonIcon(AppState.isMute);
        Debug.Log("MuteButtonOnClick: " + AppState.isMute.ToString());
    }

    private void InkognitoButtonOnClick()
    {
        AppState.isIncognito = !AppState.isIncognito;
        this.UpdateIncocnitoButtonIcon(AppState.isIncognito);
        Debug.Log("InkognitoButtonOnClick: " + AppState.isIncognito.ToString());
    }
}
