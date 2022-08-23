using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{


    
    public void GotoExploratoryRouteWalkRecording()
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
        SceneManager.LoadScene(AppState.ExploratoryRouteWalkRecodingScene);
    }
    public void GoBack()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void GotoMyExploratoryRouteWalk()
    {
        SceneManager.LoadScene(AppState.MyExploratoryRouteWalkScenes);
    }

    public void GotoUserLogin()
    {
        SceneManager.LoadScene(AppState.UserLoginScene);
    }
}