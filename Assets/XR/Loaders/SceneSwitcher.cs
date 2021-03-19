﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    void Update()
    {
        // Check if Back was pressed this frame
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GotoMenuScene();
        }
        
    }
    public void GotoARGPSScene()
    {
        SceneManager.LoadScene("ARGPS Example");
    }

    public void GotoMenuScene()
    {
        SceneManager.LoadScene("DevUI");
    }
    public void GotoProfileScene()
    {
        SceneManager.LoadScene("ProfileScene");
    }
    public void GotoErstbegehungScene()
    {
        //get button id
        var list=EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Text>();
        foreach (var df in list)
        {
            switch (df.name)
            {
                case "ID":
                    AppState.SelectedBegehung = int.Parse(df.text);
                    break;
            }
        }
        SceneManager.LoadScene("DokumentierteErstbegehung");
    }
    public void GotoErstbegehungDev()
    {
        SceneManager.LoadScene("DokumentierteErstbegehungDev");
    }
    public void GotoMeineBegehungen()
    {
        //get button id
        var list = EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Text>();
        foreach (var df in list)
        {
            switch (df.name)
            {
                case "ID":
                    AppState.SelectedWeg = int.Parse(df.text);
                    break;
            }
        }
        SceneManager.LoadScene("MeineBegehungen");
    }
    public void GotoMeineWege()
    {
        SceneManager.LoadScene("MeineWege");
    }
    public void GoBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}