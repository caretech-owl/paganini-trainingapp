using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{

    public void GotoErstbegehungDev()
    {
        SceneManager.LoadScene("ExploratoryRouteWalkRecording");
    }
    /*
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
    }*/
    public void GoBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void GotoOverview()
    {
        SceneManager.LoadScene("MyExploratoryRouteWalks");
    }

    public void GotoUserLogin()
    {
        SceneManager.LoadScene("UserLogin");
    }
}