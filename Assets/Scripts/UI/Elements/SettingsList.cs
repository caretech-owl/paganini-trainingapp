using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsList : MonoBehaviour
{

    public Button ProfilShowButton;

    public Button ProfilDeleteButton;

    public Button BegehungModeButton;

    public Button EndNavigationButton;

    // Start is called before the first frame update
    void Start()
    {
        ProfilShowButton.onClick.AddListener(() => this.ShowProfileOnClick());
        ProfilDeleteButton.onClick.AddListener(() => this.DeleteProfileOnClick());
        BegehungModeButton.onClick.AddListener(() => this.BegehungModeOnClick());
        EndNavigationButton.onClick.AddListener(() => this.EndNavigationButtonOnClick());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void ShowProfileOnClick()
    {
        SceneManager.LoadScene(AppState.startScene);
    }

    private void DeleteProfileOnClick()
    {
        // TODO Show ARE YOU SURE Dialog
        Debug.Log("Delete Profile selected");

    }

    private void BegehungModeOnClick()
    {
        // TODO Open Begehung Scene
        Debug.Log("Begehungsmodus selected");
    }

    private void EndNavigationButtonOnClick()
    {
        // TODO Reset AppState
        AppState.SelectedWeg = -1;
        AppState.lastScene = "";
        // TODO Navigate to start
        SceneManager.LoadScene(AppState.startScene);

    }
}
