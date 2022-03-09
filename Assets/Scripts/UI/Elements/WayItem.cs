using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WayItem : MonoBehaviour
{

    public Button infoContainer;

    public Text info;

    public Button startButton;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartButtonOnClick(Button selectedGameObject)
    {
        var selectedItem = selectedGameObject.transform.parent;
        this.OnClick(int.Parse(selectedItem.name));

    }
    public void ListElementOnClick(Button selectedGameObject)
    {
        var selectedItem = selectedGameObject.transform.parent;
        this.OnClick(int.Parse(selectedItem.name));
    }

    public void OnClick(int wegeId)
    {
        AppState.SelectedWeg = wegeId;
        SceneManager.LoadScene(AppState.allOkScene);
    }
}
