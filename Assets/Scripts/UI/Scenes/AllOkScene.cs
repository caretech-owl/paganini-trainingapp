using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class AllOkScene : MonoBehaviour
{

    public GameObject videoServerCheck;

    public GameObject gpsCheck;

    public GameObject akkuCheck;

    public StartButton startButton;

    public SettingButton settingButton;

    private int TEMP_CallNumber = 0;
    private int TEMP_CallNumber1 = 0;

    private IEnumerator runChecksCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        this.startButton.Disable();
        this.runChecksCoroutine = this.UpdateCheckList();
        StartCoroutine(this.runChecksCoroutine);

        // Set current scene to last scene for navigation
        AppState.lastScene = AppState.currentScene;
        AppState.currentScene = SceneSwitcher.allOkScene;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator UpdateCheckList()
    {
        bool[] checkResults = new bool[3] { false, false, false };
        while (!this.isCheckSuccessfull(checkResults))
        {
            checkResults = this.DoChecks();
            this.UpdateServerCheck(checkResults[0]);
            this.UpdateGPSCheck(checkResults[1]);
            this.UpdateAkkuCheck(checkResults[2]);

            if (this.isCheckSuccessfull(checkResults))
            {
                this.startButton.Enable();
            }

            yield return new WaitForSeconds(AppState.connectionCheckIntervall);
        }
    }

    private bool isCheckSuccessfull(bool[] checkResults)
    {
        foreach (bool result in checkResults)
        {
            if (!result)
            {
                return false;
            }
        }
        return true;
    }

    private void UpdateServerCheck(bool checkResult)
    {
        this.videoServerCheck.transform.Find("CheckIcon").gameObject.SetActive(checkResult);
    }

    private void UpdateGPSCheck(bool checkResult)
    {
        this.gpsCheck.transform.Find("CheckIcon").gameObject.SetActive(checkResult);
    }

    private void UpdateAkkuCheck(bool checkResult)
    {
        this.akkuCheck.transform.Find("CheckIcon").gameObject.SetActive(checkResult);
    }

    private bool[] DoChecks()
    {
        return new bool[] {
            this.DoCheckServer(),
            this.DoCheckGPS(),
            this.DoCheckAkku()
        };
    }

    private bool DoCheckServer()
    {
        // TODO Placeholder
        if (this.TEMP_CallNumber1 >= 3)
        {
            return true;
        }
        this.TEMP_CallNumber1++;
        return false;
    }

    private bool DoCheckGPS()
    {
        // TODO Placeholder
        if (this.TEMP_CallNumber >= 2)
        {
            return true;
        }
        this.TEMP_CallNumber++;
        return false;
    }

    private bool DoCheckAkku()
    {
        // TODO Placeholder
        return true;
    }
}