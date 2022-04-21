using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class ShowProfile : MonoBehaviour
{

    public Button BackButton;

    public GameObject Placeholder;

    public Text NicknameTag;

    // Start is called before the first frame update
    void Start()
    {
        BackButton.onClick.AddListener(() => this.BackButtonOnClick());

        this.SetNicknameText(this.GetNickname());

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void BackButtonOnClick()
    {
        Debug.Log("LOAD " + AppState.lastScene);
        SceneManager.LoadScene(AppState.settingsScene);
    }

    private string GetNickname()
    {
        // TODO Template Function
        return "Nutzer*in";
    }

    private void SetNicknameText(string name)
    {
        this.NicknameTag.text = name;
    }
}
