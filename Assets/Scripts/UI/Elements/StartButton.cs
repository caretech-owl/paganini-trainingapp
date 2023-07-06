using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    public Button infoContainer;

    public Text info;

    public Button startButton;

    // Start is called before the first frame update
    void Start()
    {
        this.infoContainer.onClick.AddListener(() => this.OnClick());
        this.startButton.onClick.AddListener(() => this.OnClick());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Enable()
    {
        this.gameObject.SetActive(true);
    }

    public void Disable()
    {
        this.gameObject.SetActive(false);
    }

    private void OnClick()
    {
        SceneManager.LoadScene(SceneSwitcher.wegeFührungScene);
    }
}
