using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using SQLite4Unity3d;

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
                    AppState.SelectedBegehung = int.Parse(df.text);
                    AppState.SelectedWeg  = int.Parse(df.text);
                    break;
            }
        }
        // check if erw present 
        List<Way> ways = DBConnector.Instance.GetConnection().Query<Way>("Select * FROM Way where Id="+AppState.SelectedWeg);
        if(ways.Count!=0){
            // TODO: add panel to ask for override

            DBConnector.Instance.GetConnection().Execute("DELETE FROM ExploratoryRouteWalk where Id="+AppState.SelectedWeg);
            DBConnector.Instance.GetConnection().Execute("DELETE FROM Pathpoint where Erw_id="+AppState.SelectedWeg);
        }

        ExploratoryRouteWalk walk=new ExploratoryRouteWalk();
        walk.Id=AppState.SelectedWeg;
        walk.Way_id=AppState.SelectedWeg;
        walk.Date= DateTime.Now;
        walk.Name= ways[0].Name;
        // We set the current exploratory walk
        AppState.currentBegehung = walk.Name;

        DBConnector.Instance.GetConnection().InsertOrReplace(walk);
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